#!/usr/bin/env bash
set -e

echo "Bootstrapping Dungeon Lord environment..."

# Check dependencies
if ! command -v godot &> /dev/null && ! command -v godot4 &> /dev/null; then
    echo "Error: godot (or godot4) not found in PATH. Please install Godot 4.2+ (C# edition)."
    exit 1
fi
if ! command -v dotnet &> /dev/null; then
    echo "Error: dotnet not found in PATH. Please install .NET 8 SDK."
    exit 1
fi
if ! command -v python3 &> /dev/null; then
    echo "Error: python3 not found in PATH."
    exit 1
fi

echo "Setting up Python virtual environment..."
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt

echo "Building .NET project..."
dotnet build DungeonLord.csproj

echo "Bootstrap complete!"
echo "To run the game, use: godot --path . --editor"
