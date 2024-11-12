#!/bin/bash

# Exit immediately if a command exits with a non-zero status
set -e

# Navigate to the C# project directory
cd TvShowLength

# Build the C# project
echo "Building the C# application..."
dotnet build -c Release

# Get the path to the compiled binary
EXECUTABLE_PATH="$(pwd)/bin/Release/net8.0/TvShowLength" 

# Check if the executable was successfully built
if [ ! -f "$EXECUTABLE_PATH" ]; then
    echo "Error: Could not find the executable. Build might have failed."
    exit 1
fi

cd ..

# Export the executable path as an environment variable
export GET_TVSHOW_TOTAL_LENGTH_BIN="$EXECUTABLE_PATH"
echo "An environment variable was set: $GET_TVSHOW_TOTAL_LENGTH_BIN"

echo "Now can run the script. For example: python3 tv-time.py < tv-shows.txt" 