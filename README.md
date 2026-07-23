# ScreenBuddy

> Your silent desktop companion for healthier screen time.

ScreenBuddy is a lightweight, privacy-first Windows desktop application built for developers, designers, writers, and professionals who spend long hours in front of a screen.

It runs silently in your system tray, timing your focus sessions. When it's time for a break, ScreenBuddy gently presents an unmissable, calm fullscreen break overlay across all connected monitors.

---

## Key Features

- 🌿 **Silent Companion** — No intrusive popups, toasts, or sound effects during focus sessions.
- 📺 **Multi-Monitor Overlay** — Fullscreen break overlay covers all connected displays with optical typography and dark mode aesthetics.
- 🔒 **Zero Network Surface** — Offline-first by design. No accounts, no cloud sync, no telemetry, no tracking.
- ⚡ **Ultra Lightweight** — Native WPF on .NET 8. Consumes ≤50 MB RAM and ≤0.5% CPU at idle.
- ⌨️ **Accessible & Thoughtful** — Full keyboard support, screen reader compatibility (Narrator), and high-contrast compliance.

---

## Installation

### Requirements
- Windows 10 (Build 1903+) or Windows 11
- 64-bit Architecture

### Direct Download
Download the latest signed setup installer from the [Releases](https://github.com/ScreenBuddy/ScreenBuddy/releases) page.

### Install via winget
```cmd
winget install ScreenBuddy.ScreenBuddy
```

---

## Building from Source

```cmd
git clone https://github.com/ScreenBuddy/ScreenBuddy.git
cd ScreenBuddy
dotnet build -c Release
```

---

## Privacy Guarantee

ScreenBuddy contains zero network client code, zero HTTP dependencies, and zero analytical trackers. All user settings are saved locally to `%APPDATA%\ScreenBuddy\config.json`.

---

## License

ScreenBuddy is open-source software licensed under the [MIT License](LICENSE).
