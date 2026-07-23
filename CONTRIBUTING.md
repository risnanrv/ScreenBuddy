# Contributing to ScreenBuddy

Thank you for your interest in contributing to ScreenBuddy!

## Core Product Philosophy
Before submitting PRs or feature proposals, please read our core product principles:
1. **Offline & Private:** ScreenBuddy will never make network requests, collect telemetry, or require user accounts.
2. **Minimal & Companion-focused:** No gamification, task management, ambient sounds, or complex dashboards.

## Getting Started
1. Install .NET 8 SDK and Visual Studio 2022 / JetBrains Rider / VS Code.
2. Clone the repository: `git clone https://github.com/ScreenBuddy/ScreenBuddy.git`.
3. Open `ScreenBuddy.sln` and build.

## Pull Request Guidelines
- Follow Conventional Commits format: `feat(scope): message` or `fix(scope): message`.
- Ensure all unit tests pass (`dotnet test`).
- Maintain zero build warnings (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).
