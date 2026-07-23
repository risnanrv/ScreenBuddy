# Changelog

All notable changes to ScreenBuddy will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-07-23

### Added
- **Silent Companion Timer Engine** — Dual-clock Stopwatch + `DateTimeOffset` hybrid countdown timer with automatic sleep/wake gap reconciliation.
- **Multi-Monitor Fullscreen Overlay** — Fullscreen break overlay rendering on all connected monitors simultaneously with optical typography, dark mode background (`#0C0E14`), and sage-mint countdown timer (`#6EE7B7`).
- **Sequential Companion Messages** — Library of 10 calm break messages rotating sequentially across break sessions.
- **System Tray Control** — Silent execution in Windows system tray with context menu controls (`Start`, `Pause`, `Resume`, `Reset`, `Settings`, `Quit`) and single-click pause/resume toggle.
- **Settings Panel** — Floating dark slate dialog to configure work duration (1–120 mins), break duration (1–60 mins), and Windows startup autorun integration.
- **First Run Onboarding** — Onboarding dialog for fresh installs.
- **Crash Recovery State Machine** — Automated snapshot persistence to recover active session state across unexpected restarts.
- **Privacy-First Architecture** — Zero network requests, zero telemetry, zero analytics, zero external API calls. All configuration saved locally in `%APPDATA%\ScreenBuddy\`.
