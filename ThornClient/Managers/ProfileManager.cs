using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ThornClient.Managers;

/// <summary>
/// Manages the active configuration profile and keeps the legacy default config folder compatible.
/// </summary>
public static class ProfileManager {
    public const string DefaultProfileName = "Default";
    private const string MetadataFileName = "profile-manifest.json";

    public static string RootConfigFolder { get; } = Path.Combine(BepInEx.Paths.ConfigPath, "ThornClient");
    public static string ActiveProfile { get; private set; } = DefaultProfileName;
    public static string CurrentProfileFolder => GetProfileFolder(ActiveProfile);

    private static readonly JsonSerializerSettings ManifestSerializerSettings = new() {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
    };

    private sealed class ProfileManifest {
        public string ActiveProfile { get; set; } = DefaultProfileName;
    }

    public static void Initialize() {
        Directory.CreateDirectory(RootConfigFolder);

        EnsureProfileDirectory(DefaultProfileName);
        EnsureManifest();

        if (!TryLoadActiveProfileFromManifest()) {
            ActiveProfile = DefaultProfileName;
            SaveManifest();
        }

        EnsureProfileDirectory(ActiveProfile);
    }

    public static string GetProfileFolder(string profileName) {
        if (string.IsNullOrWhiteSpace(profileName)) {
            profileName = DefaultProfileName;
        }

        return Path.Combine(RootConfigFolder, profileName);
    }

    public static IReadOnlyList<string> GetProfiles() {
        if (!Directory.Exists(RootConfigFolder)) return [DefaultProfileName];

        var directories = Directory.EnumerateDirectories(RootConfigFolder, "*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (directories.Count == 0) {
            directories.Add(DefaultProfileName);
        }

        return directories.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static bool HasProfile(string profileName) {
        if (string.IsNullOrWhiteSpace(profileName)) return false;
        return Directory.Exists(GetProfileFolder(profileName));
    }

    internal static void SetActiveProfile(string profileName) {
        if (string.IsNullOrWhiteSpace(profileName)) {
            profileName = DefaultProfileName;
        }

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
        if (string.IsNullOrWhiteSpace(profileName)) {
            profileName = DefaultProfileName;
        }

        if (string.Equals(ActiveProfile, profileName, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }

        var old = ActiveProfile;
        SetActiveProfile(profileName);
        try {
            ProfileSwitched?.Invoke(old, ActiveProfile);
        } catch (Exception) {
            // Listener exceptions should not break profile switching
        }

        return true;
    }

    public static void RenameProfile(string currentProfileName, string newProfileName) {
        if (string.IsNullOrWhiteSpace(currentProfileName)) {
            throw new ArgumentException("Current profile name cannot be empty.", nameof(currentProfileName));
        }

        if (string.IsNullOrWhiteSpace(newProfileName)) {
            throw new ArgumentException("New profile name cannot be empty.", nameof(newProfileName));
        }

        if (string.Equals(currentProfileName, newProfileName, StringComparison.OrdinalIgnoreCase)) {
            return;
        }

        var sourceFolder = GetProfileFolder(currentProfileName);
        if (!Directory.Exists(sourceFolder)) {
            throw new DirectoryNotFoundException($"Profile '{currentProfileName}' does not exist.");
        }

        var targetFolder = GetProfileFolder(newProfileName);
        if (Directory.Exists(targetFolder)) {
            throw new InvalidOperationException($"Profile '{newProfileName}' already exists.");
        }

        Directory.Move(sourceFolder, targetFolder);

        if (string.Equals(ActiveProfile, currentProfileName, StringComparison.OrdinalIgnoreCase)) {
            ActiveProfile = newProfileName;
            SaveManifest();
        }
    }

    public static void DeleteProfile(string profileName, bool switchToDefaultIfActive = true) {
        if (string.IsNullOrWhiteSpace(profileName)) {
            throw new ArgumentException("Profile name cannot be empty.", nameof(profileName));
        }

        if (string.Equals(profileName, DefaultProfileName, StringComparison.OrdinalIgnoreCase)) {
            throw new InvalidOperationException("The default profile cannot be deleted.");
        }

        var folder = GetProfileFolder(profileName);
        if (!Directory.Exists(folder)) {
            throw new DirectoryNotFoundException($"Profile '{profileName}' does not exist.");
        }

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

    public static void CreateProfile(string profileName, string? copyFromProfile = null) {
        if (string.IsNullOrWhiteSpace(profileName)) {
            throw new ArgumentException("Profile name cannot be empty.", nameof(profileName));
        }

        if (string.Equals(profileName, copyFromProfile, StringComparison.OrdinalIgnoreCase)) {
            throw new InvalidOperationException("A profile cannot be cloned onto itself.");
        }

        var targetFolder = GetProfileFolder(profileName);
        if (Directory.Exists(targetFolder)) {
            throw new InvalidOperationException($"Profile '{profileName}' already exists.");
        }

        if (!string.IsNullOrWhiteSpace(copyFromProfile)) {
            var sourceFolder = GetProfileFolder(copyFromProfile);
            if (!Directory.Exists(sourceFolder)) {
                throw new DirectoryNotFoundException($"Source profile '{copyFromProfile}' does not exist.");
            }

            CopyDirectory(sourceFolder, targetFolder);
            return;
        }

        Directory.CreateDirectory(targetFolder);
    }

    public static void CloneProfile(string sourceProfileName, string newProfileName) {
        CreateProfile(newProfileName, sourceProfileName);
    }

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

    private static void SaveManifest() {
        var manifestPath = Path.Combine(RootConfigFolder, MetadataFileName);
        var manifest = new ProfileManifest { ActiveProfile = ActiveProfile };
        File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest, ManifestSerializerSettings));
    }

    private static bool TryLoadActiveProfileFromManifest() {
        var manifestPath = Path.Combine(RootConfigFolder, MetadataFileName);
        if (!File.Exists(manifestPath)) return false;

        try {
            var manifestJson = File.ReadAllText(manifestPath);
            var manifest = JsonConvert.DeserializeObject<ProfileManifest>(manifestJson, ManifestSerializerSettings);
            if (manifest == null || string.IsNullOrWhiteSpace(manifest.ActiveProfile)) {
                return false;
            }

            ActiveProfile = manifest.ActiveProfile;
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
