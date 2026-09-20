# PIKATWEAKS2

A GitHub-ready Windows tweaking application inspired by the layout of modern PC optimizer apps, built around the real **PIKATWEAKS2 V10.2** tweak library.

## What is included

- 75 source-derived V10.2 library actions from the supplied `PIKATWEAKS2-V10.2.bat`.
- Native WPF desktop UI.
- Purple/dark dashboard layout.
- Search and categories.
- Apply Selected / Apply Recommended / Apply All Tweaks.
- Administrator manifest for system-wide changes.
- Hidden command execution through temporary `.cmd` files, so multi-command batch syntax is preserved without opening a visible Command Prompt.
- Activity logging under `%ProgramData%\PIKATWEAKS2\pikatweaks.log`.

## Important behavior

This project intentionally keeps the original source commands rather than claiming unsupported FPS gains. Some changes require a reboot, sign-out, or application restart before Windows reflects them.

`Apply All Tweaks` runs all 75 source-derived actions. `Apply Recommended` is a safer subset and excludes the high-impact repair/reset/power actions.

## Build on Windows

Install the .NET 8 SDK, then from the repository root:

```powershell
dotnet restore
dotnet build -c Release -p:Platform=x64
```

Publish a standalone EXE:

```powershell
dotnet publish src/PIKATWEAKS2/PIKATWEAKS2.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Output is under:

`src/PIKATWEAKS2/bin/Release/net8.0-windows/win-x64/publish/`

## GitHub Actions

Push to GitHub and the workflow in `.github/workflows/build.yml` will build and publish a Windows x64 artifact.

## License

MIT — see `LICENSE`.
