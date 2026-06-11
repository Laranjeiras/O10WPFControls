# O10WPFControls

WPF custom control library for Brazilian ERP UIs, targeting **.NET 10.0-windows**.

[![NuGet](https://img.shields.io/nuget/v/O10WPFControls)](https://www.nuget.org/packages/O10WPFControls)

---

## Installation

```
dotnet add package O10WPFControls
```

---

## Controls

### O10DatePicker

A custom date picker with calendar popup, auto-formatting input, and Portuguese localization.

**Features:**
- Editable text input with auto-mask (`dd/MM/yyyy`)
- Calendar popup with month/year navigation
- Portuguese month and weekday names (Seg, Ter, Qua, Qui, Sex, Sáb, Dom)
- Highlights today's date
- Closes popup on `Esc` or focus loss
- Readonly `IsDateValid` property — `true` when the entered date is valid
- Isolated styles — works alongside Material Design 3 (MD3) themes without visual conflicts

#### XAML usage

```xml
<Window xmlns:o10="clr-namespace:O10WPFControls.DatePicker;assembly=O10WPFControls">

    <o10:O10DatePicker
        Label="Data de nascimento"
        HasLabel="True"
        Placeholder="Selecione uma data"
        ControlHeight="40"
        SelectedDate="{Binding BirthDate, Mode=TwoWay}"
        DateSelected="OnDateSelected"
        DateChanged="OnDateChanged"/>

</Window>
```

#### Dependency Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Label` | `string` | `"Data"` | Label text displayed above the control |
| `HasLabel` | `bool` | `true` | Shows or hides the label |
| `ControlHeight` | `double` | `40` | Height of the input field |
| `BorderBrushColor` | `Brush` | `#7A8FA3` | Border color |
| `SelectedDate` | `DateTime?` | `null` | Two-way bindable selected date |
| `Placeholder` | `string` | `"Selecione uma data"` | Placeholder text when empty |
| `DateFormat` | `string` | `"dd/MM/yyyy"` | Display format |
| `IsDateValid` | `bool` | `true` | Readonly — `true` when the current text parses as a valid date |

#### Events

| Event | Args | Raised when |
|---|---|---|
| `DateSelected` | `DateSelectedEventArgs` | User picks a date from the calendar |
| `DateChanged` | `DateChangedEventArgs` | `SelectedDate` changes (typed or picked) |

---

## Build

```powershell
dotnet build
dotnet clean
```

> Requires Windows — `net10.0-windows` target is Windows-only.

---

## CI/CD

Every PR merged to `main` triggers the publish pipeline (`.github/workflows/publish-nuget.yml`):

1. Build Release
2. Pack NuGet
3. Push to [NuGet.org](https://www.nuget.org/packages/O10WPFControls)
4. Create git tag `v{version}`

**Required secret:** `NUGET_API_KEY` — NuGet.org API key with Push permission.  
Add via: GitHub repo → Settings → Secrets and variables → Actions → New repository secret.

### Versioning

Before opening a PR, bump `<Version>` in [O10WPFControls/O10WPFControls.csproj](O10WPFControls/O10WPFControls.csproj).  
The pipeline will fail if the version already exists on NuGet — this is intentional to enforce explicit version bumps.

---

## Architecture

- **Pattern:** `UserControl` with code-behind — no MVVM, no DI container.
- Controls expose `DependencyProperty` members for XAML binding.
- Converters live in `Helpers/` and implement `IValueConverter`.
- All controls follow the `O10` prefix convention — class name and `x:Class` must match exactly.

---

## License

MIT
