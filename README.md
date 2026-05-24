<div align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="https://github.com/Fluxstrap-Lab/Fluxstrap/raw/main/Images/Fluxstrap-full-dark.png">
    <source media="(prefers-color-scheme: light)" srcset="https://github.com/Fluxstrap-Lab/Fluxstrap/raw/main/Images/Fluxstrap-full-light.png">
    <img alt="Fluxstrap" src="https://github.com/Fluxstrap-Lab/Fluxstrap/raw/main/Images/Fluxstrap-full-dark.png" width="420">
  </picture>

  [![License](https://img.shields.io/github/license/Fluxstrap-Lab/Fluxstrap?color=C09104&style=flat-square)](LICENSE)
  [![Latest Release](https://img.shields.io/github/v/release/Fluxstrap-Lab/Fluxstrap?color=C09104&style=flat-square)](https://github.com/Fluxstrap-Lab/Fluxstrap/releases/latest)
  [![Downloads](https://img.shields.io/github/downloads/Fluxstrap-Lab/Fluxstrap/latest/total?color=C09104&style=flat-square)](https://github.com/Fluxstrap-Lab/Fluxstrap/releases)
  [![Discord](https://img.shields.io/discord/1093466814465110107?color=C09104&label=Discord&style=flat-square)](https://discord.gg/nKjV3mGq6R)
</div>

<br>

> [!CAUTION]
> The only official places to download Fluxstrap are this GitHub repository and [bloxstraplabs.com](https://bloxstraplabs.com). Any other websites offering downloads or claiming to be us are not owned by us.

---

<p align="center">
  <b>Fluxstrap</b> is a feature-rich third-party replacement for the standard Roblox bootstrapper.  
  A fork of <a href="https://github.com/pizzaboxer/bloxstrap">Bloxstrap</a> by <a href="https://github.com/pizzaboxer">pizzaboxer</a> with additional features and improvements.  
  <br>Supports Windows 10+.
  <br>
  <a href="https://bloxstraplabs.com"><b>Official Website</b></a> ·
  <a href="https://github.com/Fluxstrap-Lab/Fluxstrap/releases/latest"><b>Download</b></a> ·
  <a href="https://bloxstraplabs.com/wiki"><b>Wiki</b></a> ·
  <a href="https://github.com/Fluxstrap-Lab/Fluxstrap/issues"><b>Report a Bug</b></a>
</p>

---

## ✨ Features

<details open>
<summary><b>🎮 Discord Rich Presence</b></summary>
<br>
Hassle-free Discord Rich Presence integration. Let your friends know exactly what game you're playing, which server you're on, and how long you've been playing — all directly on your Discord profile.
<br><br>
<img src="https://github.com/Fluxstrap-Lab/Fluxstrap/raw/main/Images/Fluxstrap.png" width="720" alt="Discord Rich Presence Preview">
</details>

<details>
<summary><b>🎨 Modding Support</b></summary>

| Category | Included Mods |
|----------|---------------|
| **Death Sounds** | OldDeath.ogg — the classic Roblox "oof" |
| **Jump Sounds** | OldJump.mp3, OldWalk.mp3, OldGetUp.mp3 |
| **Mouse Cursors** | 10+ cursor sets: From2006, From2013, FPS Crosshair, DotCursor, WhiteDot, CleanCursor, StoofsCursor, and more |
| **Skybox** | OldAvatarBackground.rbxl — nostalgic avatar background |

Simply toggle mods on/off from the Mods page — no manual file replacement needed.
</details>

<details>
<summary><b>🌍 Server Info & Location</b></summary>
<br>
See your server's geographic location, ping, and FPS — all in real-time. Powered by <a href="https://ipinfo.io">ipinfo.io</a>.  
Includes a <b>Server Blacklist</b> feature to block servers from specific countries.
</details>

<details>
<summary><b>⚙️ Engine Configuration & Fast Flags</b></summary>
<br>
Take full control of Roblox's graphics engine with built-in presets:

| Preset | Options |
|--------|---------|
| **Texture Quality** | Auto, Low, Medium, High, Ultra |
| **MSAA Mode** | 2x, 4x, 8x |
| **FastFlag Presets** | Multiple performance/visual presets |

Includes a **Fast Flag Editor** with syntax highlighting, 12 editor themes, and live JSON validation — powered by AvalonEdit.
</details>

<details>
<summary><b>📊 Activity Tracking & Statistics</b></summary>
<br>
Track your playtime, game history, and session statistics entirely locally:

- Total playtime (today, week, month, all time)
- Most played games
- Session count and average duration
- Full play history with timestamps

All data is stored locally in JSON format — privacy-first, no telemetry.
</details>

<details>
<summary><b>🔄 Auto-Rejoin & Crash Recovery</b></summary>
<br>

| Feature | Description |
|---------|-------------|
| **Auto-Rejoin** | Automatically rejoin the same server on disconnect — configurable delay (1-60s) and max retry count |
| **Crash Recovery** | Auto-relaunch Roblox on crash with configurable cooldown |
| **Crash Log Viewer** | Browse and inspect Roblox crash logs directly inside Fluxstrap |
</details>

<details>
<summary><b>🔒 GoodbyeDPI Integration</b></summary>
<br>
Built-in DPI/SNI bypass integration for Turkey and other restrictive networks.  
Manage GoodbyeDPI directly from Fluxstrap — no separate configuration needed.

- Auto-start with Fluxstrap
- Custom parameter support
- Run as a service option
</details>

<details>
<summary><b>🖥️ Bootstrapper Styles</b></summary>
<br>
Choose from <b>8 different bootstrapper styles</b> for the Roblox launch dialog:

| Style | Description |
|-------|-------------|
| **FluentDialog** | Modern Windows 11 Fluent Design with Mica effect |
| **ClassicFluentDialog** | Compact Fluent variant |
| **VistaDialog** | Windows Vista-style dialog |
| **LegacyDialog2008** | Nostalgic 2008 Roblox style |
| **LegacyDialog2011** | 2011 Roblox style |
| **ProgressDialog** | Minimal progress bar |
| **ByfronDialog** | Byfron/Hyperion themed |
| **CustomDialog** | Fully customizable via XML theme templates |

Includes a built-in **Bootstrapper Editor** for creating and previewing custom themes.
</details>

<details>
<summary><b>📦 Version Manager</b></summary>
<br>
Install, switch, and manage multiple Roblox versions simultaneously:

- Channel selection: Stable, RC, RCPlayerTest, ZCookie, and more
- Version history browsing
- One-click version switching
- Per-version settings
- Version backup and restore
</details>

<details>
<summary><b>🔌 Custom Integrations</b></summary>
<br>
Launch external programs and scripts automatically alongside Roblox:

- Discord, OBS, GPU software, or any custom program
- Delayed startup support
- Auto-close on Roblox exit
- Per-integration toggle
</details>

<details>
<summary><b>🎮 In-Game Overlay</b></summary>
<br>
A lightweight in-game overlay showing:

- FPS counter
- Ping / latency
- Server info
- Current time

Toggle with a configurable hotkey (default: Scroll Lock). Minimal performance impact.
</details>

<details>
<summary><b>🛡️ Server Blacklist</b></summary>
<br>
Block Roblox servers from specific countries. Useful for avoiding high-ping servers or regional restrictions.
</details>

<details>
<summary><b>📡 Server Status Dashboard</b></summary>
<br>
Live Roblox server status dashboard — check if Roblox is operational before launching.
</details>

<details>
<summary><b>⌨️ Customizable Shortcuts</b></summary>
<br>
Full keyboard shortcut customization with remappable key bindings.
</details>

<details>
<summary><b>🌐 Multi-Language Support</b></summary>
<br>
15+ language translations including English, Turkish, German, French, Spanish, Italian, Russian, Portuguese, Dutch, Polish, Japanese, Korean, Chinese (Simplified & Traditional), Arabic, and more.
</details>

---

## 📥 Download

| Method | Description |
|--------|-------------|
| **📦 Installer (Recommended)** | [Download latest setup](https://github.com/Fluxstrap-Lab/Fluxstrap/releases/latest) — InnoSetup installer, handles everything automatically |
| **📄 Portable EXE** | Download the single-file portable executable from the [releases page](https://github.com/Fluxstrap-Lab/Fluxstrap/releases) — no installation required |
| **🛠️ Build from source** | See instructions below |

**Requirements:** [.NET 6 Desktop Runtime](https://aka.ms/dotnet-core-applaunch?missing_runtime=true&arch=x64&rid=win11-x64&apphost_version=6.0.16&gui=true) on Windows 10+ (build 19041+)

---

## 🛠️ Building from Source

### Prerequisites
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- Git

### Steps

```powershell
# Clone the repository with submodules
git clone --recursive https://github.com/Fluxstrap-Lab/Fluxstrap.git
cd Fluxstrap

# Debug build
dotnet build Fluxstrap\Fluxstrap.csproj

# Release build
dotnet build Fluxstrap\Fluxstrap.csproj -c Release

# Publish single-file executable
.\publish.ps1                          # self-contained (~162 MB)
.\publish.ps1 -FrameworkDependent      # requires .NET runtime (~17 MB)
```

The project uses a [forked WPF UI library](https://github.com/bloxstraplabs/wpfui) (cloned as a submodule under `wpfui/`).

### Project Structure

```
Fluxstrap/
├── App.xaml.cs              # Application entry point
├── Bootstrapper.cs          # Core Roblox download/launch logic
├── LaunchHandler.cs         # Launch mode routing
├── Watcher.cs               # In-game background process
├── Installer.cs             # First-time setup & updates
├── Models/                  # API models, entities, JSON persistable data
├── UI/Elements/             # XAML pages & windows
│   ├── Settings/            # Settings window & pages
│   ├── Bootstrapper/        # Bootstrapper dialog styles
│   ├── Installer/           # Setup wizard
│   ├── Editor/              # Theme editor
│   ├── Overlay/             # In-game overlay
│   ├── Dialogs/             # Dialog windows
│   └── About/               # About window
├── UI/ViewModels/           # MVVM ViewModels
├── UI/Converters/           # XAML value converters
├── Integrations/            # Discord RPC, ActivityWatcher, Cleaner
├── Utility/                 # HTTP, Filesystem, Shortcuts, etc.
└── Resources/               # Images, fonts, mod files, translations
```

---

## 📄 License

Fluxstrap is licensed under the **MIT License**.  
See [LICENSE](LICENSE) for details.

Original Bloxstrap by [pizzaboxer](https://github.com/pizzaboxer) — all credit to the original project and its contributors.

## 💬 Support

- 📖 [Wiki & Documentation](https://bloxstraplabs.com/wiki)
- 🐛 [Report an Issue](https://github.com/Fluxstrap-Lab/Fluxstrap/issues)
- 💬 [Join our Discord](https://discord.gg/nKjV3mGq6R)
- 🔗 [Official Website](https://bloxstraplabs.com)

## 🤝 Contributing

Contributions are welcome! Feel free to:
- Submit pull requests
- Report bugs and suggest features via issues
- Help with translations
- Spread the word!

---

<div align="center">
  <sub>Built with ❤️ by the Fluxstrap community</sub>
  <br>
  <sub>Not affiliated with Roblox Corporation</sub>
</div>
