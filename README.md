# PIKATWEAKS2

A native Windows WPF optimizer UI inspired by modern PC-tweaking apps, built around the 75 source-derived PikaTweaks V10.2 library actions.

## What it does

- Purple/dark dashboard UI with sidebar navigation.
- 75 real Windows actions derived from `PikaTweaks-V10.2.bat`.
- Search and category filtering.
- Recommended one-click optimization pack.
- Apply selected and Apply All Non-Maintenance operations.
- Automatic registry backup before a tweak batch.
- Manual backup and latest-backup restore.
- Hidden command execution; no visible Command Prompt window.
- Administrator UAC via application manifest.
- Activity log and restart indicators.
- Diagnostics and repair actions are separated from normal optimization.

## Build

Install Visual Studio 2022 with **.NET desktop development** or the .NET 8 SDK on Windows.

```powershell
dotnet restore
dotnet publish PIKATWEAKS2.sln -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

The executable is `publish/PIKATWEAKS2.exe`.

## GitHub Actions

The included workflow builds the self-contained Windows executable and uploads it as an artifact.

## Notes

This application changes Windows registry, power, networking, services, privacy and maintenance settings. Create a backup before changing settings and review each action. Hardware/Windows-version behavior can differ, and FPS/performance gains are not guaranteed.
