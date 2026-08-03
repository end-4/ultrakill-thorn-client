#!/bin/bash

#
# make-package.sh
#
# Builds the mod, collects package files and assets, and creates a zip package.
#
# Usage: run this script from the repository root.
# Parameters:
#   $1: OutputDir - Directory where the final zip will be written (default: script folder)
#   $2: Configuration - Build configuration (default: Release)
#

# Exit immediately if a command exits with a non-zero status.
set -e

# --- Helper Functions ---
print_info() {
    echo "$1"
}

print_warning() {
    echo "WARNING: $1"
}

# --- Configuration & Setup ---

# Use provided arguments or fall back to defaults.
OUTPUT_DIR=${1:-"."}
CONFIGURATION=${2:-"Release"}

ROOT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
TMP_PACKAGE_BUILD_DIR="package_build"
STAGING_DIR="$ROOT_DIR/$TMP_PACKAGE_BUILD_DIR"
PLUGIN_DIR="$STAGING_DIR/plugins"
ASSEMBLY_NAME="ThornClient.dll"
DOC_NAME="ThornClient.xml"

print_info "Repository root: $ROOT_DIR"

# 1) Prepare staging directory
if [ -d "$STAGING_DIR" ]; then
    print_info "Removing existing staging folder: $STAGING_DIR"
    rm -rf "$STAGING_DIR"
fi
mkdir -p "$PLUGIN_DIR"

# 2) Build the mod and copy it
MOD_FOLDER="$ROOT_DIR"
ASSEMBLY_PATH="$MOD_FOLDER/ThornClient/bin/$CONFIGURATION/netstandard2.1/$ASSEMBLY_NAME"
DOC_PATH="$MOD_FOLDER/ThornClient/bin/$CONFIGURATION/netstandard2.1/$DOC_NAME"

print_info "Building mod in '$MOD_FOLDER' using configuration: $CONFIGURATION"
dotnet build -c "$CONFIGURATION"

cp "$ASSEMBLY_PATH" "$PLUGIN_DIR/"
cp "$DOC_PATH" "$PLUGIN_DIR/"

# 3) Copy all files from package folder into staging
PACKAGE_FOLDER="$ROOT_DIR/package"
if [ ! -d "$PACKAGE_FOLDER" ]; then
    echo "ERROR: package folder not found at $PACKAGE_FOLDER" >&2
    exit 1
fi
print_info "Copying package files from '$PACKAGE_FOLDER' to staging"
cp -r "$PACKAGE_FOLDER"/* "$STAGING_DIR/"

# Try to read name/version from manifest.json for zip naming
MANIFEST_PATH="$PACKAGE_FOLDER/manifest.json"
PKG_NAME="package"
PKG_VER=$(date +%Y%m%d%H%M%S)

if [ -f "$MANIFEST_PATH" ]; then
    # Use jq to parse json, it's a common and robust tool.
    # The '|| true' prevents the script from exiting if jq fails or a key is null.
    MANIFEST_NAME=$(jq -r '.name' "$MANIFEST_PATH" || true)
    MANIFEST_VER=$(jq -r '.version_number' "$MANIFEST_PATH" || true)

    if [[ -n "$MANIFEST_NAME" && "$MANIFEST_NAME" != "null" ]]; then
        PKG_NAME="$MANIFEST_NAME"
    else
        print_warning "Could not read 'name' from manifest.json."
    fi
    if [[ -n "$MANIFEST_VER" && "$MANIFEST_VER" != "null" ]]; then
        PKG_VER="$MANIFEST_VER"
    else
        print_warning "Could not read 'version_number' from manifest.json. Falling back to timestamp."
    fi
else
    print_warning "Could not find manifest.json for name/version. Falling back to timestamped name."
fi

# 4) Create BepInEx/plugins/assets and copy assets
ASSETS_SRC="$ROOT_DIR/assets"
ASSETS_DEST="$STAGING_DIR/plugins/assets"
print_info "Creating assets destination: $ASSETS_DEST"
mkdir -p "$ASSETS_DEST"
if [ -d "$ASSETS_SRC" ] && [ -n "$(ls -A "$ASSETS_SRC")" ]; then
    print_info "Copying assets from '$ASSETS_SRC' to '$ASSETS_DEST'"
    cp -r "$ASSETS_SRC"/* "$ASSETS_DEST/"
else
    print_warning "Assets folder not found or is empty at: $ASSETS_SRC"
fi

# 4.1) Copy main icon to plugin dir
cp "$STAGING_DIR/icon.png" "$PLUGIN_DIR/icon.png"

# 5) Create zip package
ZIP_NAME="$PKG_NAME-$PKG_VER.zip"
ZIP_PATH="$(cd "$OUTPUT_DIR" && pwd)/$ZIP_NAME" # Ensure ZIP_PATH is absolute
if [ -f "$ZIP_PATH" ]; then print_info "Removing existing zip: $ZIP_PATH"; rm "$ZIP_PATH"; fi
print_info "Creating zip: $ZIP_PATH"

# Compress everything inside the staging folder
(cd "$STAGING_DIR" && zip -r "$ZIP_PATH" .)

print_info "Package created at: $ZIP_PATH"
print_info "Staging folder retained at: $STAGING_DIR (remove if not needed)"

# Return path for scripts / automation
echo "$ZIP_PATH"
