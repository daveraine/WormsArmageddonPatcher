# Worms Armageddon Team File Patcher

A simple command line tool to unlock all utilities, weapons, cheats, and the Full Wormage scheme in Worms Armageddon.

## Usage

```
WormsArmageddonPatcher.exe "<path\to\WG.WGT>"
```

This can easily be invoked by dragging the `WG.WGT` file onto `WormsArmageddonPatcher.exe`.

## Building

Release build for Win x64.

```
dotnet publish -r win-x64 -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true
```