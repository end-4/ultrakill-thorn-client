#!/bin/bash

# Port of setup-libs.ps1 to Bash for Linux environments.

# Exit immediately if a command exits with a non-zero status.
set -e

# --- Helper Functions ---
print_error() {
    echo "ERROR: $1" >&2
}

print_warning() {
    echo "WARNING: $1"
}

print_info() {
    echo "$1"
}

# --- Configuration ---

# Use provided paths or fall back to defaults.
# Usage: ./setup-libs.sh [ULTRAKILL_PATH] [R2MODMAN_PROFILE_PATH]
ULTRAKILL_PATH=${1:-"$HOME/.local/share/Steam/steamapps/common/ULTRAKILL"}
R2MODMAN_PROFILE_PATH=${2:-"$HOME/.local/share/com.kesomannen.gale/ultrakill/profiles/Default"}

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
TARGET_DIR="$SCRIPT_DIR/libs"

print_info "--- Library Setup ---"
print_info "Target directory: $TARGET_DIR"

# Ensure the target directory exists
if [ ! -d "$TARGET_DIR" ]; then
    print_info "Creating target directory..."
    mkdir -p "$TARGET_DIR"
fi

# 1) Copy Managed DLLs from ULTRAKILL installation
UK_MANAGED_DIR="$ULTRAKILL_PATH/ULTRAKILL_Data/Managed"
if [ ! -d "$UK_MANAGED_DIR" ]; then
    print_error "ULTRAKILL Managed directory not found at: $UK_MANAGED_DIR"
    echo "Please ensure the path points to the root of your ULTRAKILL installation."
    exit 1
fi

UK_DLLS=(
    "Assembly-CSharp.dll"
    "Newtonsoft.Json.dll"
    "Unity.Addressables.dll"
    "Unity.InputSystem.dll"
    "Unity.ResourceManager.dll"
    "Unity.TextMeshPro.dll"
    "UnityEngine.dll"
    "UnityEngine.AssetBundleModule.dll"
    "UnityEngine.AudioModule.dll"
    "UnityEngine.CoreModule.dll"
    "UnityEngine.IMGUIModule.dll"
    "UnityEngine.InputLegacyModule.dll"
    "UnityEngine.InputModule.dll"
    "UnityEngine.UI.dll"
    "UnityEngine.UIModule.dll"
    "UnityEngine.UnityWebRequestModule.dll"
    "UnityEngine.PhysicsModule.dll"
    "UnityEngine.TextRenderingModule.dll"
)

for dll in "${UK_DLLS[@]}"; do
    source_file="$UK_MANAGED_DIR/$dll"
    if [ -f "$source_file" ]; then
        cp "$source_file" "$TARGET_DIR/"
        print_info "Successfully copied: $dll"
    else
        print_error "Source file not found: $source_file"
    fi
done

# 2) Copy Plugin DLLs from r2modman profile
if [ ! -d "$R2MODMAN_PROFILE_PATH" ]; then
    print_warning "r2modman profile directory not found at: $R2MODMAN_PROFILE_PATH"
    print_warning "Ensure your plugins are installed and the path is correct."
fi

R2_DLLS=(
    "BepInEx/core/BepInEx.dll"
    "BepInEx/core/0Harmony.dll"
    "BepInEx/plugins/end_4-NukeLib/NukeLib.dll"
    "BepInEx/plugins/end_4-NukeLib/NukeLib.xml"
    "BepInEx/plugins/NukeLib/NukeLib.dll"
    "BepInEx/plugins/NukeLib/NukeLib.xml"
    "BepInEx/plugins/end_4-Notiffy/Notiffy/Notiffy.dll"
    "BepInEx/plugins/end_4-Notiffy/Notiffy/Notiffy.xml"
)

for dll in "${R2_DLLS[@]}"; do
    source_file="$R2MODMAN_PROFILE_PATH/$dll"
    if [ -f "$source_file" ]; then
        cp "$source_file" "$TARGET_DIR/"
        print_info "Successfully copied: $(basename "$dll")"
    else
        print_error "Source file not found: $source_file"
    fi
done

print_info $'\nSetup finished. If any errors occurred, please specify your paths manually.'
print_info "You can now build the mod using make-package.sh."
