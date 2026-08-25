using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ThornClient.Managers;

/// <summary>
/// Manages the active configuration profile and keeps the legacy default config folder compatible.
/// </summary>
public static class ProfileManager {
    /// <summary>
    /// The default profile name
    /// </summary>
    public const string DefaultProfileName = "Default";

    private const string MetadataFileName = "profile-manifest.json";

    /// <summary>
    /// The config folder of Thorn
    /// </summary>
    public static string RootConfigFolder { get; } = Path.Combine(BepInEx.Paths.ConfigPath, "ThornClient");

    /// <summary>
    /// Name of currently used profile
    /// </summary>
    public static string ActiveProfile { get; private set; } = DefaultProfileName;

    /// <summary>
    /// The folder of the currently used profile
    /// </summary>
    public static string CurrentProfileFolder => GetProfileFolder(ActiveProfile);

    /// <summary>
    /// Emitted when changes to the selected profile or available profiles happen on disk
    /// </summary>
    public static event Action? ProfilesChanged;

    private static readonly JsonSerializerSettings ManifestSerializerSettings = new() {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
    };

    private static FileSystemWatcher? _profileWatcher;
    private static readonly object ProfileWatcherSync = new();
    private static bool _isWritingManifest;

    private sealed class ProfileManifest {
        public string ActiveProfile { get; set; } = DefaultProfileName;
    }

    /// <summary>
    /// Initializes the profile manager
    /// </summary>
    public static void Initialize() {
        Directory.CreateDirectory(RootConfigFolder);

        EnsureProfileDirectory(DefaultProfileName);
        EnsureManifest();
        EnsureWatcher();

        if (!TryLoadActiveProfileFromManifest()) {
            ActiveProfile = DefaultProfileName;
            SaveManifest();
        }

        EnsureProfileDirectory(ActiveProfile);
    }

    /// <summary>
    /// Gets the full path to a profile's folder
    /// </summary>
    /// <param name="profileName">The profile name</param>
    /// <returns>The full path to the profile's folder</returns>
    public static string GetProfileFolder(string profileName) {
        if (string.IsNullOrWhiteSpace(profileName)) profileName = DefaultProfileName;
        return Path.Combine(RootConfigFolder, profileName);
    }

    /// <summary>
    /// Gets names of all profiles
    /// </summary>
    /// <returns>A read-only list of profile name strings</returns>
    public static IReadOnlyList<string> GetProfiles() {
        if (!Directory.Exists(RootConfigFolder)) return [DefaultProfileName];

        var directories = Directory.EnumerateDirectories(RootConfigFolder, "*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (directories.Count == 0) directories.Add(DefaultProfileName);

        return directories.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// Check whether a profile exists
    /// </summary>
    /// <param name="profileName">The profile name</param>
    /// <returns>True if the profile exists, false otherwise</returns>
    public static bool HasProfile(string profileName) {
        if (string.IsNullOrWhiteSpace(profileName)) return false;
        return Directory.Exists(GetProfileFolder(profileName));
    }

    /// <summary>
    /// Lower level stuff within this class when switching profiles
    /// </summary>
    /// <param name="profileName">The profile name</param>
    internal static void SetActiveProfile(string profileName) {
        if (string.IsNullOrWhiteSpace(profileName)) profileName = DefaultProfileName;
        EnsureProfileDirectory(profileName);
        ActiveProfile = profileName;
        SaveManifest();
    }

    /// <summary>
    /// The event fired when the profile changes.
    /// The first argument is the old profile name, and the second is the new profile name.
    /// </summary>
    public static event Action<string, string>? ProfileSwitched;

    /// <summary>
    /// Switches the active profile and notifies listeners if the profile actually changed.
    /// Returns true if a switch occurred, false if the requested profile was already active.
    /// </summary>
    public static bool SwitchProfile(string profileName) {
        if (string.IsNullOrWhiteSpace(profileName)) profileName = DefaultProfileName;
        if (string.Equals(ActiveProfile, profileName, StringComparison.OrdinalIgnoreCase)) return false;

        var old = ActiveProfile;
        SetActiveProfile(profileName);
        try {
            ProfileSwitched?.Invoke(old, ActiveProfile);
        } catch (Exception) {
            // Listener exceptions should not break profile switching
        }

        return true;
    }

    /// <summary>
    /// Renames a profile
    /// </summary>
    /// <param name="currentProfileName">Name of target profile</param>
    /// <param name="newProfileName">New name to rename the target profile to</param>
    /// <exception cref="ArgumentException">When the profile name(s) are faulty</exception>
    /// <exception cref="DirectoryNotFoundException">When the target profile doesn't exist</exception>
    /// <exception cref="InvalidOperationException">When the target name is of an existing profile</exception>
    public static void RenameProfile(string currentProfileName, string newProfileName) {
        if (string.IsNullOrWhiteSpace(currentProfileName))
            throw new ArgumentException("Curr profile name can't be empty");
        if (string.IsNullOrWhiteSpace(newProfileName)) throw new ArgumentException("New profile name cannot be empty.");
        if (string.Equals(currentProfileName, newProfileName, StringComparison.OrdinalIgnoreCase)) return;

        var sourceFolder = GetProfileFolder(currentProfileName);
        if (!Directory.Exists(sourceFolder))
            throw new DirectoryNotFoundException($"Profile '{currentProfileName}' does not exist.");

        var targetFolder = GetProfileFolder(newProfileName);
        if (Directory.Exists(targetFolder))
            throw new InvalidOperationException($"Profile '{newProfileName}' already exists.");

        Directory.Move(sourceFolder, targetFolder);

        if (string.Equals(ActiveProfile, currentProfileName, StringComparison.OrdinalIgnoreCase)) {
            ActiveProfile = newProfileName;
            SaveManifest();
        }
    }

    /// <summary>
    /// Deletes a profile
    /// </summary>
    /// <param name="profileName">Name of the profile to delete</param>
    /// <param name="switchToDefaultIfActive">Whether to switch to the default profile if the deleted profile is active</param>
    /// <exception cref="ArgumentException">When the given profile name is faulty</exception>
    /// <exception cref="InvalidOperationException">When you try to delete the default profile</exception>
    /// <exception cref="DirectoryNotFoundException">When the profile doesn't exist</exception>
    public static void DeleteProfile(string profileName, bool switchToDefaultIfActive = true) {
        if (string.IsNullOrWhiteSpace(profileName))
            throw new ArgumentException("Profile name cannot be empty.", nameof(profileName));
        if (string.Equals(profileName, DefaultProfileName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The default profile cannot be deleted.");

        var folder = GetProfileFolder(profileName);
        if (!Directory.Exists(folder))
            throw new DirectoryNotFoundException($"Profile '{profileName}' does not exist.");

        if (string.Equals(ActiveProfile, profileName, StringComparison.OrdinalIgnoreCase)) {
            if (switchToDefaultIfActive) {
                ActiveProfile = DefaultProfileName;
                SaveManifest();
            } else {
                throw new InvalidOperationException(
                    "The active profile cannot be deleted without switching away from it."
                );
            }
        }

        Directory.Delete(folder, recursive: true);
    }

    /// <summary>
    /// Creates a new profile
    /// </summary>
    public static void CreateProfile() {
        var profileName = GetSafeProfileName("New");
        CreateProfile(profileName);
    }

    /// <summary>
    /// Creates a new profile
    /// </summary>
    /// <param name="profileName">Name of the new profile. Might incrementally fallback'd if the name already exists.</param>
    /// <param name="copyFromProfile">The profile to copy from. Optional, leave blank for default settings.</param>
    /// <exception cref="ArgumentException">When the profile name is faulty</exception>
    /// <exception cref="InvalidOperationException">When you try to clone a profile into itself</exception>
    /// <exception cref="DirectoryNotFoundException">When the source profile for cloning is not found</exception>
    public static void CreateProfile(string profileName, string? copyFromProfile = null) {
        if (string.IsNullOrWhiteSpace(profileName))
            throw new ArgumentException("Profile name cannot be empty.", nameof(profileName));
        if (string.Equals(profileName, copyFromProfile, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("A profile cannot be cloned onto itself.");

        var targetFolder = GetProfileFolder(profileName);
        if (Directory.Exists(targetFolder))
            throw new InvalidOperationException($"Profile '{profileName}' already exists.");

        if (!string.IsNullOrWhiteSpace(copyFromProfile)) {
            var sourceFolder = GetProfileFolder(copyFromProfile);
            if (!Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException($"Source profile '{copyFromProfile}' does not exist.");

            CopyDirectory(sourceFolder, targetFolder);
            return;
        }

        Directory.CreateDirectory(targetFolder);
    }

    /// <summary>
    /// Gets a profile name that does not already exist
    /// </summary>
    /// <param name="baseName">The base name, such as "New profile"</param>
    /// <returns>A safe profile name that doesn't already exist</returns>
    public static string GetSafeProfileName(string baseName) {
        if (string.IsNullOrWhiteSpace(baseName)) baseName = "New";

        var candidateName = baseName;
        var index = 1;

        while (Directory.Exists(GetProfileFolder(candidateName))) {
            candidateName = $"{baseName} ({index++})";
        }

        return candidateName;
    }

    /// <summary>
    /// Clones a profile
    /// </summary>
    /// <param name="sourceProfileName">The name of the profile to clone from</param>
    /// <exception cref="ArgumentException">When the source profile name is faulty</exception>
    /// <exception cref="DirectoryNotFoundException">When the source profile doesn't exist</exception>
    public static void CloneProfile(string sourceProfileName) {
        if (string.IsNullOrWhiteSpace(sourceProfileName))
            throw new ArgumentException("Source profile name cannot be empty.", nameof(sourceProfileName));

        var sourceFolder = GetProfileFolder(sourceProfileName);
        if (!Directory.Exists(sourceFolder))
            throw new DirectoryNotFoundException($"Source profile '{sourceProfileName}' does not exist.");

        CloneProfile(sourceProfileName, GetSafeProfileName(sourceProfileName));
    }

    /// <summary>
    /// Clones a profile
    /// </summary>
    /// <param name="sourceProfileName">The name of the profile to clone from</param>
    /// <param name="newProfileName">The name of the new profile to clone into</param>
    public static void CloneProfile(string sourceProfileName, string newProfileName) {
        CreateProfile(newProfileName, sourceProfileName);
    }

    /// <summary>
    /// Ensures a profile's folder exists
    /// </summary>
    /// <param name="profileName">The name of the profile</param>
    public static void EnsureProfileDirectory(string profileName) {
        var folder = GetProfileFolder(profileName);
        Directory.CreateDirectory(folder);
    }

    private static void EnsureManifest() {
        var manifestPath = Path.Combine(RootConfigFolder, MetadataFileName);
        if (File.Exists(manifestPath)) return;

        var manifest = new ProfileManifest { ActiveProfile = DefaultProfileName };
        File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest, ManifestSerializerSettings));
    }

    private static void EnsureWatcher() {
        lock (ProfileWatcherSync) {
            if (_profileWatcher != null) return;

            _profileWatcher = new FileSystemWatcher(RootConfigFolder) {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite |
                               NotifyFilters.CreationTime,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true,
            };

            _profileWatcher.Changed += OnConfigurationChanged;
            _profileWatcher.Created += OnConfigurationChanged;
            _profileWatcher.Deleted += OnConfigurationChanged;
            _profileWatcher.Renamed += OnConfigurationChanged;
            _profileWatcher.Error += (_, _) => { };
        }
    }

    private static void OnConfigurationChanged(object sender, FileSystemEventArgs args) {
        if (_isWritingManifest)
            return;
        if (string.Equals(Path.GetFileName(args.FullPath), MetadataFileName, StringComparison.OrdinalIgnoreCase))
            TryApplyManifestProfile();

        try {
            ProfilesChanged?.Invoke();
        } catch (Exception e) {
            Plugin.Log.LogError($"[ProfileManager] Failed to invoke ProfilesChanged : {e}");
        }
    }

    private static void TryApplyManifestProfile() {
        if (!TryGetManifestActiveProfile(out var desiredProfile))
            return;
        if (!Directory.Exists(GetProfileFolder(desiredProfile)))
            return;
        if (string.Equals(ActiveProfile, desiredProfile, StringComparison.OrdinalIgnoreCase))
            return;

        var oldProfile = ActiveProfile;
        ActiveProfile = desiredProfile;
        try {
            ProfileSwitched?.Invoke(oldProfile, ActiveProfile);
        } catch (Exception) {
            // Listener exceptions should not break profile switching
        }
    }

    private static void SaveManifest() {
        var manifestPath = Path.Combine(RootConfigFolder, MetadataFileName);
        var manifest = new ProfileManifest { ActiveProfile = ActiveProfile };

        _isWritingManifest = true;
        try {
            File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest, ManifestSerializerSettings));
        } finally {
            _isWritingManifest = false;
        }
    }

    private static bool TryLoadActiveProfileFromManifest() {
        if (!TryGetManifestActiveProfile(out var profileName))
            return false;
        if (!Directory.Exists(GetProfileFolder(profileName)))
            return false;

        ActiveProfile = profileName;
        return true;
    }

    private static bool TryGetManifestActiveProfile(out string profileName) {
        profileName = string.Empty;

        var manifestPath = Path.Combine(RootConfigFolder, MetadataFileName);
        if (!File.Exists(manifestPath)) return false;

        try {
            var manifestJson = File.ReadAllText(manifestPath);
            var manifest = JsonConvert.DeserializeObject<ProfileManifest>(manifestJson, ManifestSerializerSettings);
            if (manifest == null || string.IsNullOrWhiteSpace(manifest.ActiveProfile)) {
                return false;
            }

            profileName = manifest.ActiveProfile;
            return true;
        } catch (Exception) {
            return false;
        }
    }

    private static void CopyDirectory(string sourceFolder, string targetFolder) {
        Directory.CreateDirectory(targetFolder);

        foreach (var file in Directory.EnumerateFiles(sourceFolder, "*", SearchOption.AllDirectories)) {
            var relativePath = Path.GetRelativePath(sourceFolder, file);
            var destinationPath = Path.Combine(targetFolder, relativePath);
            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDirectory)) {
                Directory.CreateDirectory(destinationDirectory);
            }

            File.Copy(file, destinationPath, overwrite: true);
        }
    }
}
