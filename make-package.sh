#!/bin/bash

#
# make-package.sh
#
# Builds the mod projects, collects package files and assets, and creates zip packages for both targets.
#
# Usage: run this script from the repository root.
# Parameters:
#   $1: OutputDir - Directory where the final zips will be written (default: script folder)
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
OUTPUT_DIR=${1:-"."}
CONFIGURATION=${2:-"Release"}
ROOT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

print_info "Repository root: $ROOT_DIR"

# 1) Build the solution
print_info "Building solution using configuration: $CONFIGURATION"
(cd "$ROOT_DIR" && dotnet build -c "$CONFIGURATION")

# 2) Define packaging targets: Name, ProjectDir, PackageDir, AssemblyName, DocName, IncludeAssets
TARGETS=(
    "Thorn_Core:ThornClient:package:ThornClient.dll:ThornClient.xml:true"
    "Thorn:ThornClientModules:package_modules:ThornClientModules.dll:ThornClientModules.xml:false"
)

CREATED_ZIPS=()

for TARGET in "${TARGETS[@]}"; do
    IFS=":" read -r T_NAME T_PROJECT_DIR T_PACKAGE_DIR T_ASSEMBLY_NAME T_DOC_NAME T_INCLUDE_ASSETS <<< "$TARGET"

    print_info ""
    print_info "--- Packaging $T_NAME ---"
    STAGING_DIR="$ROOT_DIR/package_build/$T_NAME"
    PLUGIN_DIR="$STAGING_DIR/plugins/$T_NAME"

    # Prepare staging directory
    if [ -d "$STAGING_DIR" ]; then
        print_info "Removing existing staging folder: $STAGING_DIR"
        rm -rf "$STAGING_DIR"
    fi
    mkdir -p "$PLUGIN_DIR"

    # Copy binary and xml documentation if present
    ASSEMBLY_PATH="$ROOT_DIR/$T_PROJECT_DIR/bin/$CONFIGURATION/netstandard2.1/$T_ASSEMBLY_NAME"
    DOC_PATH="$ROOT_DIR/$T_PROJECT_DIR/bin/$CONFIGURATION/netstandard2.1/$T_DOC_NAME"

    if [ -f "$ASSEMBLY_PATH" ]; then
        print_info "Copying binary: $ASSEMBLY_PATH"
        cp "$ASSEMBLY_PATH" "$PLUGIN_DIR/"
    else
        echo "ERROR: Assembly not found at $ASSEMBLY_PATH" >&2
        exit 1
    fi

    if [ -f "$DOC_PATH" ]; then
        print_info "Copying documentation: $DOC_PATH"
        cp "$DOC_PATH" "$PLUGIN_DIR/"
    fi

    # Copy all files from package folder into staging
    PACKAGE_FOLDER="$ROOT_DIR/$T_PACKAGE_DIR"
    if [ ! -d "$PACKAGE_FOLDER" ]; then
        echo "ERROR: Package folder not found at $PACKAGE_FOLDER" >&2
        exit 1
    fi
    print_info "Copying package files from '$PACKAGE_FOLDER' to staging"
    cp -r "$PACKAGE_FOLDER"/* "$STAGING_DIR/"

    # Try to read name/version from manifest.json for zip naming
    MANIFEST_PATH="$PACKAGE_FOLDER/manifest.json"
    PKG_NAME="$T_NAME"
    PKG_VER=$(date +%Y%m%d%H%M%S)

    if [ -f "$MANIFEST_PATH" ]; then
        MANIFEST_NAME=$(jq -r '.name' "$MANIFEST_PATH" 2>/dev/null || true)
        MANIFEST_VER=$(jq -r '.version_number' "$MANIFEST_PATH" 2>/dev/null || true)

        if [[ -n "$MANIFEST_NAME" && "$MANIFEST_NAME" != "null" ]]; then
            PKG_NAME="$MANIFEST_NAME"
        else
            print_warning "Could not read 'name' from manifest.json in '$PACKAGE_FOLDER'."
        fi
        if [[ -n "$MANIFEST_VER" && "$MANIFEST_VER" != "null" ]]; then
            PKG_VER="$MANIFEST_VER"
        else
            print_warning "Could not read 'version_number' from manifest.json in '$PACKAGE_FOLDER'. Falling back to timestamp."
        fi
    else
        print_warning "Could not find manifest.json for name/version in '$PACKAGE_FOLDER'. Falling back to default name/timestamp."
    fi

    # Copy assets if requested
    if [ "$T_INCLUDE_ASSETS" = "true" ]; then
        ASSETS_SRC="$ROOT_DIR/assets"
        ASSETS_DEST="$PLUGIN_DIR/assets"
        print_info "Creating assets destination: $ASSETS_DEST"
        mkdir -p "$ASSETS_DEST"
        if [ -d "$ASSETS_SRC" ] && [ -n "$(ls -A "$ASSETS_SRC" 2>/dev/null)" ]; then
            print_info "Copying assets from '$ASSETS_SRC' to '$ASSETS_DEST'"
            cp -r "$ASSETS_SRC"/* "$ASSETS_DEST/"
        else
            print_warning "Assets folder not found or is empty at: $ASSETS_SRC"
        fi
    fi

    # Copy main icon to plugin dir
    if [ -f "$STAGING_DIR/icon.png" ]; then
        cp "$STAGING_DIR/icon.png" "$PLUGIN_DIR/icon.png"
    fi

    # Create zip package
    ZIP_NAME="$PKG_NAME-$PKG_VER.zip"
    ZIP_PATH="$(cd "$OUTPUT_DIR" && pwd)/$ZIP_NAME"
    if [ -f "$ZIP_PATH" ]; then
        print_info "Removing existing zip: $ZIP_PATH"
        rm "$ZIP_PATH"
    fi
    print_info "Creating zip: $ZIP_PATH"

    # Compress everything inside the staging folder
    (cd "$STAGING_DIR" && zip -r "$ZIP_PATH" .)

    print_info "Package created at: $ZIP_PATH"
    print_info "Staging folder retained at: $STAGING_DIR (remove if not needed)"

    CREATED_ZIPS+=("$ZIP_PATH")
done

# Return paths for scripts / automation
for ZIP in "${CREATED_ZIPS[@]}"; do
    echo "$ZIP"
done
