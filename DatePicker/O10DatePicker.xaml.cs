using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;
using System.Text.RegularExpressions;

namespace O10WPFControls.DatePicker;
    public partial class O10DatePicker : UserControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(O10DatePicker),
                new PropertyMetadata("Data"));

        public static readonly DependencyProperty HasLabelProperty =
            DependencyProperty.Register("HasLabel", typeof(bool), typeof(O10DatePicker),
                new PropertyMetadata(true));

        public static readonly DependencyProperty ControlHeightProperty =
            DependencyProperty.Register("ControlHeight", typeof(double), typeof(O10DatePicker),
                new PropertyMetadata(40.0));

        public static readonly DependencyProperty BorderBrushColorProperty =
            DependencyProperty.Register("BorderBrushColor", typeof(Brush), typeof(O10DatePicker),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(122, 143, 163))));

        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register("SelectedDate", typeof(DateTime?), typeof(O10DatePicker),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedDateChanged));

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(O10DatePicker),
                new PropertyMetadata("Selecione uma data"));

        public static readonly DependencyProperty DateFormatProperty =
            DependencyProperty.Register("DateFormat", typeof(string), typeof(O10DatePicker),
                new PropertyMetadata("dd/MM/yyyy"));

        public static readonly DependencyProperty IsDateValidProperty =
            DependencyProperty.Register("IsDateValid", typeof(bool), typeof(O10DatePicker),
                new PropertyMetadata(true));

        #endregion

        #region Properties

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public bool HasLabel
        {
            get => (bool)GetValue(HasLabelProperty);
            set => SetValue(HasLabelProperty, value);
        }

        public double ControlHeight
        {
            get => (double)GetValue(ControlHeightProperty);
            set => SetValue(ControlHeightProperty, value);
        }

        public Brush BorderBrushColor
        {
            get => (Brush)GetValue(BorderBrushColorProperty);
            set => SetValue(BorderBrushColorProperty, value);
        }

        public DateTime? SelectedDate
        {
            get => (DateTime?)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public string DateFormat
        {
            get => (string)GetValue(DateFormatProperty);
            set => SetValue(DateFormatProperty, value);
        }

        public bool IsDateValid
        {
            get => (bool)GetValue(IsDateValidProperty);
            private set => SetValue(IsDateValidProperty, value);
        }

        public string DateText
        {
            get => txtDate.Text;
            set => txtDate.Text = value;
        }

        #endregion

        private DateTime _currentDate;
        private bool _updatingText;
        private readonly string[] _months = {
            "janeiro", "fevereiro", "março", "abril", "maio", "junho",
            "julho", "agosto", "setembro", "outubro", "novembro", "dezembro"
        };

        public O10DatePicker()
        {
            InitializeComponent();
            _currentDate = DateTime.Today;
            txtDate.Text = Placeholder;
            RenderCalendar();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        #region Events

        public event EventHandler<DateSelectedEventArgs>? DateSelected;
        public event EventHandler<DateChangedEventArgs>? DateChanged;

        #endregion

        #region Popup Focus Management

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
                window.PreviewMouseDown += OnWindowPreviewMouseDown;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
                window.PreviewMouseDown -= OnWindowPreviewMouseDown;
        }

        private void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!calendarPopup.IsOpen)
                return;
            var source = e.OriginalSource as DependencyObject;
            if (source == null)
                return;
            if (!IsVisualDescendant(calendarPopup.Child, source) && !IsVisualDescendant(this, source))
                calendarPopup.IsOpen = false;
        }

        private static bool IsVisualDescendant(DependencyObject? parent, DependencyObject child)
        {
            var current = child;
            while (current != null)
            {
                if (current == parent)
                    return true;
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }

        private void CalendarBorder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                calendarPopup.IsOpen = false;
                e.Handled = true;
            }
        }

        #endregion

        #region Input Handling

        private void TxtDate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[0-9/]");
        }

        private void TxtDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingText) return;
            var text = txtDate.Text;

            _updatingText = true;
            try
            {
                if (text.Length == 2 && !text.Contains("/"))
                {
                    txtDate.Text = text + "/";
                    txtDate.CaretIndex = txtDate.Text.Length;
                    return;
                }

                if (text.Length == 5 && text.Count(c => c == '/') == 1)
                {
                    txtDate.Text = text + "/";
                    txtDate.CaretIndex = txtDate.Text.Length;
                    return;
                }
            }
            finally
            {
                _updatingText = false;
            }

            txtDate.CaretIndex = txtDate.Text.Length;
            ValidateDate(text);
        }

        private void TxtDate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                return;
            }
            if (e.Key == Key.Escape)
            {
                calendarPopup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void TxtDate_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtDate.Text == Placeholder)
            {
                _updatingText = true;
                txtDate.Text = "";
                _updatingText = false;
                return;
            }
            txtDate.SelectAll();
        }

        private void TxtDate_LostFocus(object sender, RoutedEventArgs e)
        {
            var text = txtDate.Text;
            if (string.IsNullOrEmpty(text))
            {
                _updatingText = true;
                txtDate.Text = Placeholder;
                _updatingText = false;
                SelectedDate = null;
                IsDateValid = true;
                return;
            }
            ValidateDate(text);
        }

        private void ValidateDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text == Placeholder)
            {
                SelectedDate = null;
                IsDateValid = true;
                return;
            }

            if (DateTime.TryParseExact(text, DateFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                SelectedDate = date;
                IsDateValid = true;
                _currentDate = date;
                RenderCalendar();
                OnDateChanged(date);
                return;
            }

            IsDateValid = false;
        }

        #endregion

        #region Calendar Rendering

        private void BtnCalendar_Click(object sender, RoutedEventArgs e)
        {
            calendarPopup.IsOpen = true;
        }

        private void RenderCalendar()
        {
            txtMonthYear.Text = $"{_months[_currentDate.Month - 1]} de {_currentDate.Year}";

            var firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(_currentDate.Year, _currentDate.Month);

            int dayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            dayOfWeek = dayOfWeek == 0 ? 6 : dayOfWeek - 1;

            daysGrid.Items.Clear();

            var previousMonthDays = DateTime.DaysInMonth(
                _currentDate.Year == 1 ? 12 : _currentDate.Year,
                _currentDate.Month == 1 ? 12 : _currentDate.Month - 1);

            for (int i = dayOfWeek - 1; i >= 0; i--)
            {
                daysGrid.Items.Add(CreateDayButton(previousMonthDays - i, true));
            }

            for (int day = 1; day <= daysInMonth; day++)
            {
                var button = CreateDayButton(day, false);

                if (SelectedDate.HasValue &&
                    SelectedDate.Value.Day == day &&
                    SelectedDate.Value.Month == _currentDate.Month &&
                    SelectedDate.Value.Year == _currentDate.Year)
                {
                    button.Background = new SolidColorBrush(Color.FromRgb(25, 118, 210));
                    button.Foreground = Brushes.White;
                    button.FontWeight = FontWeights.Bold;
                }

                if (day == DateTime.Today.Day &&
                    _currentDate.Month == DateTime.Today.Month &&
                    _currentDate.Year == DateTime.Today.Year)
                {
                    button.BorderBrush = new SolidColorBrush(Color.FromRgb(25, 118, 210));
                    button.BorderThickness = new Thickness(2);
                }

                daysGrid.Items.Add(button);
            }

            int totalCells = dayOfWeek + daysInMonth;
            int remainingCells = 42 - totalCells;

            for (int day = 1; day <= remainingCells; day++)
            {
                daysGrid.Items.Add(CreateDayButton(day, true));
            }
        }

        private Button CreateDayButton(int day, bool isOtherMonth)
        {
            var button = new Button
            {
                Style = (Style)FindResource("O10DayButtonStyle"),
                Content = day.ToString(),
                Background = isOtherMonth ? Brushes.Transparent : Brushes.White,
                Foreground = isOtherMonth ? new SolidColorBrush(Color.FromRgb(200, 200, 200)) : Brushes.Black,
                Tag = day
            };

            button.Click += (s, e) => SelectDay(day);

            if (!isOtherMonth)
            {
                button.MouseEnter += (s, e) =>
                {
                    if (!IsSelectedDay(button))
                        button.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                };

                button.MouseLeave += (s, e) =>
                {
                    if (!IsSelectedDay(button))
                        button.Background = Brushes.White;
                };
            }

            return button;
        }

        private static bool IsSelectedDay(Button button) =>
            button.Background is SolidColorBrush b && b.Color == Color.FromRgb(25, 118, 210);

        private void SelectDay(int day)
        {
            try
            {
                var date = new DateTime(_currentDate.Year, _currentDate.Month, day);
                SelectedDate = date;
                txtDate.Text = date.ToString(DateFormat);
                calendarPopup.IsOpen = false;
                RenderCalendar();
                OnDateSelected(date);
                OnDateChanged(date);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Data inválida: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentDate.AddMonths(-1);
            RenderCalendar();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentDate.AddMonths(1);
            RenderCalendar();
        }

        #endregion

        #region Event Handlers

        private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is O10DatePicker picker)
            {
                picker._updatingText = true;
                try
                {
                    picker.txtDate.Text = e.NewValue is DateTime date
                        ? date.ToString(picker.DateFormat)
                        : picker.Placeholder;
                }
                finally
                {
                    picker._updatingText = false;
                }
            }
        }

        private void OnDateSelected(DateTime date)
        {
            DateSelected?.Invoke(this, new DateSelectedEventArgs(date));
        }

        private void OnDateChanged(DateTime date)
        {
            DateChanged?.Invoke(this, new DateChangedEventArgs(date));
        }

        #endregion

        #region Public Methods

        public void Clear()
        {
            SelectedDate = null;
            txtDate.Text = Placeholder;
            IsDateValid = true;
        }

        public void SetDate(DateTime date)
        {
            SelectedDate = date;
            _currentDate = date;
            txtDate.Text = date.ToString(DateFormat);
            RenderCalendar();
        }

        #endregion
    }

    #region Event Args

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

    #endregion
