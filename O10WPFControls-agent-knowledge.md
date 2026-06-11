# O10WPFControls — Agent Knowledge Base

> Knowledge document for AI agents implementing O10WPFControls in a target WPF application.  
> Source: `O10WPFControls` POC library — `.NET 10.0-windows`, pure WPF, no external NuGet packages.

---

## 1. What This Library Is

A WPF custom control library targeting **net10.0-windows**. Contains reusable `UserControl` components for Brazilian ERP UIs. No MVVM framework, no external packages — pure WPF with code-behind and `DependencyProperty`.

**Package metadata**

| Field | Value |
|---|---|
| PackageId | `O10WPFControls` |
| Version | `1.0.6` |
| TargetFramework | `net10.0-windows` |
| Author | Carlos Alexandre Laranjeiras |

---

## 2. File Structure to Copy

```
O10WPFControls/
├── DatePicker/
│   ├── O10DatePicker.xaml          ← UserControl markup
│   └── O10DatePicker.xaml.cs       ← Code-behind + EventArgs classes
├── Helpers/
│   └── BoolToVisibilityConverter.cs
└── O10WPFControls.csproj
```

---

## 3. Integration Options

### Option A — Reference the compiled DLL

Add a `<Reference>` or `<ProjectReference>` pointing to `O10WPFControls.dll` (or the `.csproj`).

```xml
<!-- In the target .csproj -->
<ProjectReference Include="..\O10WPFControls\O10WPFControls.csproj" />
```

### Option B — Copy source files directly

Copy the two directories (`DatePicker/` and `Helpers/`) into the target project. Adjust namespaces if the target assembly differs from `O10WPFControls`.

> **Namespace rule:** The XAML `x:Class` and the C# class namespace must match exactly.  
> If you change the assembly name, update both `x:Class="O10WPFControls.DatePicker.O10DatePicker"` and `namespace O10WPFControls.DatePicker;` together.

---

## 4. Available Controls

### 4.1 `O10DatePicker`

**Namespace:** `O10WPFControls.DatePicker`  
**File:** `DatePicker/O10DatePicker.xaml` + `DatePicker/O10DatePicker.xaml.cs`

A date picker with:
- Floating label (optional)
- Auto-formatted text input (`dd/MM/yyyy` with auto-inserted slashes)
- Calendar popup with Portuguese month names (week starts on Monday)
- Navigation between months (‹ / ›)
- Highlighted selected day (blue #1976D2)
- Readonly `IsDateValid` feedback
- `Enter` moves focus to next field; `Escape` closes popup

---

## 5. Dependency Properties — O10DatePicker

| Property | Type | Default | Description |
|---|---|---|---|
| `Label` | `string` | `"Data"` | Text shown above the control |
| `HasLabel` | `bool` | `true` | Show/hide the label (`BoolToVisibilityConverter`) |
| `ControlHeight` | `double` | `40.0` | Height of the input border |
| `BorderBrushColor` | `Brush` | `#7A8FA3` | Border color of the input container |
| `SelectedDate` | `DateTime?` | `null` | Two-way bindable selected date |
| `Placeholder` | `string` | `"Selecione uma data"` | Text shown when no date is set |
| `DateFormat` | `string` | `"dd/MM/yyyy"` | Format used for display and parsing |
| `IsDateValid` | `bool` | `true` | **Readonly** — `true` when field is empty or contains a valid date |
| `DateText` | `string` | — | **CLR-only** — direct read/write access to the raw `TextBox` text (no DP, no binding support) |

> `IsDateValid` has a `private set` on the CLR wrapper — the host cannot write to it, only read/bind.

---

## 6. Events — O10DatePicker

| Event | Args type | When fired |
|---|---|---|
| `DateSelected` | `DateSelectedEventArgs` | User clicks a day in the calendar popup |
| `DateChanged` | `DateChangedEventArgs` | Any time the resolved date changes (typing or calendar) |

Both `EventArgs` classes live in the same file (`O10DatePicker.xaml.cs`):

```csharp
public class DateSelectedEventArgs : EventArgs
{
    public DateTime Date { get; }
    public DateSelectedEventArgs(DateTime date) => Date = date;
}

public class DateChangedEventArgs : EventArgs
{
    public DateTime Date { get; }
    public DateChangedEventArgs(DateTime date) => Date = date;
}
```

---

## 7. Public Methods — O10DatePicker

```csharp
// Clears the selection and resets text to Placeholder
picker.Clear();

// Programmatically sets a date (updates text, calendar view, and SelectedDate)
picker.SetDate(new DateTime(2025, 6, 9));
```

---

## 8. XAML Usage in the Target Window/UserControl

### 8.1 Declare the namespace

```xml
xmlns:o10="clr-namespace:O10WPFControls.DatePicker;assembly=O10WPFControls"
```

> If using Option B (source copy in the same assembly), omit `;assembly=O10WPFControls`.

### 8.2 Minimal usage

```xml
<o10:O10DatePicker />
```

### 8.3 Full usage with bindings

```xml
<o10:O10DatePicker
    Label="Data de Nascimento"
    HasLabel="True"
    ControlHeight="44"
    BorderBrushColor="#7A8FA3"
    Placeholder="dd/mm/aaaa"
    DateFormat="dd/MM/yyyy"
    SelectedDate="{Binding DataNascimento, Mode=TwoWay}"
    DateSelected="OnDateSelected"
    DateChanged="OnDateChanged" />
```

### 8.4 Without label

```xml
<o10:O10DatePicker HasLabel="False" SelectedDate="{Binding DataEmissao, Mode=TwoWay}" />
```

### 8.5 Validating in the host

```csharp
// Read IsDateValid before form submission
if (!datePicker.IsDateValid)
{
    MessageBox.Show("Informe uma data válida.");
    return;
}
var date = datePicker.SelectedDate; // DateTime? — null when empty
```

---

## 9. Required Helper — BoolToVisibilityConverter

**File:** `Helpers/BoolToVisibilityConverter.cs`  
**Namespace:** `O10WPFControls`

```csharp
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(...)  { throw new NotImplementedException(); }
}
```

The converter is registered **inline inside `O10DatePicker.xaml`** as a `UserControl.Resources` entry — **no app-level `ResourceDictionary` needed**:

```xml
<UserControl.Resources>
    <helpers:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
</UserControl.Resources>
```

The XAML namespace alias used inside the control:
```xml
xmlns:helpers="clr-namespace:O10WPFControls"
```

> If source is copied into a different assembly/namespace, update this alias to match.

---

## 10. Isolated Styles (Theme Isolation)

All internal WPF styles (`O10TextBoxStyle`, `O10CalendarButtonStyle`, `O10NavButtonStyle`, `O10DayButtonStyle`) set `OverridesDefaultStyle="True"`. This prevents third-party themes such as **MaterialDesignInXaml** from overriding the control's visual appearance. No action is needed on the host side — the control is self-contained.

---

## 11. Behavior Details

### Auto-formatting (typing)

| Typed chars | Result |
|---|---|
| `09` | `09/` (slash auto-inserted) |
| `09/06` | `09/06/` (slash auto-inserted) |
| `09/06/2025` | triggers `ValidateDate` → sets `SelectedDate` |

Only digits and `/` are accepted (`PreviewTextInput` blocks everything else).

### Calendar week layout

Week starts **Monday**. The grid is always 6 rows × 7 columns (42 cells). Days from the previous and next month fill empty cells in a muted gray (`#C8C8C8`). Clicking a grayed-out cell selects that day number in the *current* displayed month (known limitation — it does not navigate to the adjacent month).

### Month names (hardcoded PT-BR)

```
janeiro, fevereiro, março, abril, maio, junho,
julho, agosto, setembro, outubro, novembro, dezembro
```

No `CultureInfo` localization — changing locale does not affect display.

### `SelectedDate` two-way binding

When the host sets `SelectedDate` programmatically (e.g., loading a form), the `OnSelectedDateChanged` static callback updates `txtDate.Text` automatically. Setting it to `null` resets the text to `Placeholder`.

---

## 12. Known Issues (POC State)

| # | Issue | Status |
|---|---|---|
| 1 | Clicking a grayed-out (other-month) day does not navigate to that month | Open |
| 2 | `MouseLeave` hover-reset uses `!=` on `SolidColorBrush` instances — brush comparison by reference fails | **Fixed** — `IsSelectedDay` now compares via `b.Color == Color.FromRgb(25, 118, 210)` |
| 3 | Month names and week day headers are hardcoded PT-BR — not driven by `CultureInfo` | By design (POC) |
| 4 | `RenderCalendar()` year-bug: when navigating back from January, `DateTime.DaysInMonth` receives `_currentDate.Year` instead of `_currentDate.Year - 1` for the previous December — trailing cells may be off by one | Open |

---

## 13. Checklist for the Target Project

- [ ] Target framework is `net10.0-windows` (or compatible) with `<UseWPF>true</UseWPF>`
- [ ] `ProjectReference` or DLL reference added to `O10WPFControls`
- [ ] XAML namespace alias declared on the Window/UserControl that uses the control
- [ ] If source-copied: `BoolToVisibilityConverter` namespace alias inside `O10DatePicker.xaml` matches the target assembly namespace
- [ ] `SelectedDate` binding uses `Mode=TwoWay` when two-way sync is needed
- [ ] Form validation reads `IsDateValid` and/or checks `SelectedDate == null` before submitting

---

*Generated from source: `O10WPFControls` v1.0.6 — 2026-06-11*
