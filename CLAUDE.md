# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

`O10WPFControls` is a **WPF custom control library POC** targeting `.NET 10.0-windows`. It contains reusable WPF `UserControl` components for Brazilian ERP UIs. No external NuGet packages — pure WPF framework.

## Build

```powershell
dotnet build
dotnet clean
```

There are no test projects. No application entry point — this is a control library (`.csproj` with `<UseWPF>true</UseWPF>`).

## Architecture

- **Pattern:** WPF UserControl with code-behind (not MVVM). Controls expose `DependencyProperty` members for XAML binding and raise typed custom events.
- **No DI container, no ViewModels** — controls are self-contained.
- **Converters** live in `Helpers/` and must implement `IValueConverter`.

### Naming convention

Controls follow the `O10` prefix: class name and XAML `x:Class` must match exactly (e.g., `O10DatePicker` in both XAML and `.cs`). Mismatch causes XAML parsing failures at runtime.

## DatePicker control (`DatePicker/`)

`O10DatePicker` is a custom date picker with:
- Configurable label, placeholder, border color, and height via `DependencyProperty`
- Calendar popup with Portuguese month names and keyboard/mouse navigation
- Auto-formatting text input (`dd/MM/yyyy`)
- Readonly `IsDateValid` dependency property
- Events: `DateSelected` (`DateSelectedEventArgs`) and `DateChanged` (`DateChangedEventArgs`)

Key dependency properties: `Label`, `HasLabel`, `ControlHeight`, `BorderBrushColor`, `SelectedDate`, `Placeholder`, `DateFormat`, `IsDateValid`.

## CI/CD (GitHub Actions)

Pipeline: `.github/workflows/publish-nuget.yml` (at repo root)
- **Trigger:** every PR merged to `main`
- **Runner:** `windows-latest` (required for `net10.0-windows`)
- **Steps:** build Release → pack → push to NuGet.org → create git tag `v{version}`
- **Requires GitHub secret:** `NUGET_API_KEY` — NuGet.org API key with Push permission for `O10WPFControls`
  - Add via: GitHub repo → Settings → Secrets and variables → Actions → New repository secret

### Versioning workflow

Before opening a PR, bump `<Version>` in `O10WPFControls.csproj`. The pipeline reads it, publishes the package, and tags the commit automatically. If the version is not bumped, the NuGet push step fails (version already exists) — intentional to enforce explicit bumps.

---

## Known issues (POC state)

1. `RenderCalendar()` year-bug: when navigating back from January, `DateTime.DaysInMonth` receives the current year instead of the previous year — causes incorrect trailing-cell count. (`O10DatePicker.xaml.cs`, line ~305)
2. Portuguese month/day names are hardcoded — no CultureInfo-based localization.
3. `BoolToVisibilityConverter` lives in the root namespace `O10WPFControls` instead of `O10WPFControls.Helpers` — minor namespace/folder mismatch, not a runtime error.
