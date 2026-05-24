> [!CAUTION]
> The only official places to download Fluxstrap are this GitHub repository and [bloxstraplabs.com](https://bloxstraplabs.com). Any other websites offering downloads or claiming to be us are not owned by us.

<p align="center">
    <img src="https://github.com/bloxstraplabs/Fluxstrap/raw/main/Images/Fluxstrap-full-dark.png#gh-dark-mode-only" width="380">
    <img src="https://github.com/bloxstraplabs/Fluxstrap/raw/main/Images/Fluxstrap-full-light.png#gh-light-mode-only" width="380">
</p>

<div align="center">

[![License][shield-repo-license]][repo-license]
[![Version][shield-repo-latest]][repo-latest]
[![Downloads][shield-repo-releases]][repo-releases]

</div>

----

Fluxstrap is a third-party replacement for the standard Roblox bootstrapper with a bunch of extra features. It is a fork of [Bloxstrap](https://github.com/pizzaboxer/bloxstrap) by pizzaboxer.

Running into a problem or need help? [Submit an issue](https://github.com/bloxstraplabs/Fluxstrap/issues).

Fluxstrap is supported for PCs running Windows.

## Features

| Category | Feature |
|----------|---------|
| **Rich Presence** | Hassle-free Discord Rich Presence — let your friends know what game you're playing |
| **Modding** | Simple content file modding support (death sounds, mouse cursors, skyboxes, etc.) |
| **Server Info** | See your server's geographic location (courtesy of [ipinfo.io](https://ipinfo.io)) |
| **Engine Config** | Configure graphics fidelity, UI experience, and fast flags |
| **Activity Tracking** | Track playtime, game history, and statistics |
| **Auto-Rejoin** | Automatically rejoin servers on disconnect |
| **Crash Recovery** | Auto-relaunch on crash with configurable cooldown |
| **Server Blacklist** | Block servers from specific countries |
| **Custom Integrations** | Launch external programs alongside Roblox |
| **GoodbyeDPI** | Built-in DPI/SNI bypass integration for Turkey and restrictive networks |
| **Server Status** | Live Roblox server status dashboard |
| **Version Manager** | Install, switch, and manage multiple Roblox versions |
| **Crash Log Viewer** | Browse and inspect Roblox crash logs in-app |
| **System Info** | View system specs and Roblox installation details |
| **Custom Integrations** | Launch other programs/scripts alongside Roblox automatically |
| **Appearance** | Multiple bootstrapper styles (Legacy 2008, 2011, Vista, Classic, Glass, Byfron) |
| **Shortcuts** | Customizable keyboard shortcuts |
| **Multi-language** | 15+ language translations (English, Turkish, German, French, Spanish, Italian, Russian, Portuguese, Dutch, Polish, Japanese, Korean, Chinese, Arabic, and more) |

## Installing

Download the [latest release](https://github.com/anomalyco/Fluxstrap/releases/latest) and run the setup, or download the portable single-file EXE.

Requires [.NET 6 Desktop Runtime](https://aka.ms/dotnet-core-applaunch?missing_runtime=true&arch=x64&rid=win11-x64&apphost_version=6.0.16&gui=true) on Windows 10+.

## Building from source

```powershell
# Prerequisites: .NET 6 SDK
git clone --recursive https://github.com/anomalyco/Fluxstrap
cd Fluxstrap

# Build
dotnet build Fluxstrap\Fluxstrap.csproj

# Publish single-file portable EXE
.\publish.ps1                          # self-contained (~162 MB)
.\publish.ps1 -FrameworkDependent      # requires .NET runtime (~17 MB)
```

The project uses a [forked WPF UI library](https://github.com/bloxstraplabs/wpfui) (cloned as a submodule under `wpfui/`).

## Code signing

Code signing for official releases is provided by [SignPath.io](https://signpath.io/).

## License

Fluxstrap is licensed under the MIT License. See [LICENSE](LICENSE) for details.

[shield-repo-license]:  https://img.shields.io/github/license/bloxstraplabs/Fluxstrap
[shield-repo-releases]: https://img.shields.io/github/downloads/bloxstraplabs/Fluxstrap/latest/total?color=981bfe
[shield-repo-latest]:   https://img.shields.io/github/v/release/bloxstraplabs/Fluxstrap?color=7a39fb

[repo-license]:  https://github.com/bloxstraplabs/Fluxstrap/blob/main/LICENSE
[repo-releases]: https://github.com/bloxstraplabs/Fluxstrap/releases
[repo-latest]:   https://github.com/bloxstraplabs/Fluxstrap/releases/latest
