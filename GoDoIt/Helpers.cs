using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace GoDoIt.ViewModels;

public class ColorToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Avalonia.Media.Color c ? new SolidColorBrush(c) : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is SolidColorBrush b ? b.Color : AvaloniaProperty.UnsetValue;
}

public class CalendarView : UserControl
{
    private Point _dragStart;

    public static readonly StyledProperty<ObservableCollection<EventViewModel>?> TasksProperty =
        AvaloniaProperty.Register<CalendarView, ObservableCollection<EventViewModel>?>(nameof(Tasks));

    public static readonly StyledProperty<DateTime> SelectedDateProperty =
        AvaloniaProperty.Register<CalendarView, DateTime>(nameof(SelectedDate), DateTime.Today);

    public static readonly StyledProperty<ICommand?> SelectTaskCommandProperty =
        AvaloniaProperty.Register<CalendarView, ICommand?>(nameof(SelectTaskCommand));

    public ObservableCollection<EventViewModel>? Tasks
    {
        get => GetValue(TasksProperty);
        set => SetValue(TasksProperty, value);
    }

    public DateTime SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public ICommand? SelectTaskCommand
    {
        get => GetValue(SelectTaskCommandProperty);
        set => SetValue(SelectTaskCommandProperty, value);
    }

    static CalendarView()
    {
        TasksProperty.Changed.AddClassHandler<CalendarView>((v, e) =>
        {
            if (e.OldValue is ObservableCollection<EventViewModel> old)
                old.CollectionChanged -= v.OnTasksChanged;
            if (e.NewValue is ObservableCollection<EventViewModel> newCol)
                newCol.CollectionChanged += v.OnTasksChanged;
            v.Rebuild();
        });
        SelectedDateProperty.Changed.AddClassHandler<CalendarView>((v, _) => v.Rebuild());
        SelectTaskCommandProperty.Changed.AddClassHandler<CalendarView>((v, _) => v.Rebuild());
    }

    private void OnTasksChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        => Rebuild();

    public CalendarView() => Rebuild();

    private void Rebuild()
    {
        var date = SelectedDate;
        var firstDay = new DateTime(date.Year, date.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        int startDow = (int)firstDay.DayOfWeek;

        var root = new Grid { RowDefinitions = new RowDefinitions("Auto,Auto,*") };

        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto") };

        var prevBtn = new Button { Content = "◀", Padding = new Thickness(8, 4) };
        prevBtn.Click += (_, _) => SelectedDate = date.AddMonths(-1);

        var nextBtn = new Button { Content = "▶", Padding = new Thickness(8, 4) };
        nextBtn.Click += (_, _) => SelectedDate = date.AddMonths(1);

        var monthCombo = new ComboBox
        {
            ItemsSource = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                            .Take(12).ToArray(),
            SelectedIndex = date.Month - 1,
            FontSize = 16,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        monthCombo.SelectionChanged += (_, _) =>
        {
            if (monthCombo.SelectedIndex >= 0 && monthCombo.SelectedIndex + 1 != SelectedDate.Month)
                SelectedDate = new DateTime(SelectedDate.Year, monthCombo.SelectedIndex + 1, 1);
        };

        var yearCombo = new ComboBox
        {
            ItemsSource = Enumerable.Range(DateTime.Today.Year - 5, 11).ToArray(),
            SelectedItem = date.Year,
            FontSize = 16,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        yearCombo.SelectionChanged += (_, _) =>
        {
            if (yearCombo.SelectedItem is int year && year != SelectedDate.Year)
                SelectedDate = new DateTime(year, SelectedDate.Month, 1);
        };

        var monthYearPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Center,
            Children = { monthCombo, yearCombo }
        };

        Grid.SetColumn(prevBtn, 0);
        Grid.SetColumn(monthYearPanel, 1);
        Grid.SetColumn(nextBtn, 2);
        header.Children.Add(prevBtn);
        header.Children.Add(monthYearPanel);
        header.Children.Add(nextBtn);
        Grid.SetRow(header, 0);
        root.Children.Add(header);

        var dowRow = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*,*,*,*,*,*") };
        string[] dayNames = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
        for (int i = 0; i < 7; i++)
        {
            var tb = new TextBlock
            {
                Text = dayNames[i],
                FontSize = 12,
                FontWeight = FontWeight.Medium,
                Foreground = new SolidColorBrush(Color.Parse("#888")),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 4)
            };
            Grid.SetColumn(tb, i);
            dowRow.Children.Add(tb);
        }
        Grid.SetRow(dowRow, 1);
        root.Children.Add(dowRow);

        var calGrid = new Grid();
        for (int r = 0; r < 6; r++) calGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        for (int c = 0; c < 7; c++) calGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        int cellIndex = 0;
        for (int r = 0; r < 6; r++)
        {
            for (int c = 0; c < 7; c++, cellIndex++)
            {
                int dayNum = cellIndex - startDow + 1;
                bool isCurrentMonth = dayNum >= 1 && dayNum <= daysInMonth;
                DateTime? cellDate = isCurrentMonth ? new DateTime(date.Year, date.Month, dayNum) : null;

                var cell = BuildCell(dayNum, isCurrentMonth, cellDate);
                Grid.SetRow(cell, r);
                Grid.SetColumn(cell, c);
                calGrid.Children.Add(cell);
            }
        }
        Grid.SetRow(calGrid, 2);
        root.Children.Add(calGrid);

        Content = root;
    }

    private Border BuildCell(int dayNum, bool isCurrentMonth, DateTime? cellDate)
    {
        bool isToday = cellDate.HasValue && cellDate.Value.Date == DateTime.Today;
        bool isSelected = cellDate.HasValue && cellDate.Value.Date == SelectedDate.Date;

        var sp = new StackPanel { Margin = new Thickness(2) };

        var dayLabel = new TextBlock
        {
            Text = isCurrentMonth ? dayNum.ToString() : string.Empty,
            FontSize = 12,
            FontWeight = isToday ? FontWeight.Bold : FontWeight.Normal,
            Foreground = isCurrentMonth
                ? (isToday ? new SolidColorBrush(Color.Parse("#5b4fcf")) : new SolidColorBrush(Color.Parse("#333")))
                : new SolidColorBrush(Color.Parse("#ccc")),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 2, 4, 0)
        };
        sp.Children.Add(dayLabel);

        if (cellDate.HasValue && Tasks != null)
        {
            foreach (var evm in Tasks.Where(t => t.Event.DueOn(cellDate.Value)))
            {
                var captured = evm;
                var chip = new Border
                {
                    Background = new SolidColorBrush(evm.Color),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(4, 1),
                    Margin = new Thickness(2, 1),
                    Cursor = new Cursor(StandardCursorType.Hand),
                    Child = new TextBlock
                    {
                        Text = evm.Title,
                        FontSize = 10,
                        Foreground = Brushes.White,
                        TextWrapping = TextWrapping.NoWrap
                    }
                };

                chip.PointerPressed += (_, e) =>
                {
                    var vm = DataContext as MainWindowViewModel;
                    if (vm is null)
                        return;

                    var occurrenceVm = new EventViewModel(captured.Event, vm.Categories, cellDate);
                    vm.DraggedEvent = occurrenceVm;
                    _dragStart = e.GetPosition(this);
                    SelectTaskCommand?.Execute(occurrenceVm);
                };

                chip.PointerMoved += async (_, e) =>
                {
                    var vm = DataContext as MainWindowViewModel;
                    if (vm?.DraggedEvent is null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                        return;

                    var current = e.GetPosition(this);

                    if (Math.Abs(current.X - _dragStart.X) < 5 &&
                        Math.Abs(current.Y - _dragStart.Y) < 5)
                        return;

                    chip.Opacity = 0.5;
                    await DragDrop.DoDragDropAsync(e, new DataTransfer(), DragDropEffects.Move);
                    chip.Opacity = 1.0;
                };

                sp.Children.Add(chip);
            }
        }

        IBrush normalBackground = isSelected
            ? new SolidColorBrush(Color.Parse("#EDE7FF"))
            : Brushes.Transparent;

        IBrush hoverBackground = new SolidColorBrush(Color.Parse("#D6F5E3"));

        var border = new Border
        {
            BorderBrush = new SolidColorBrush(Color.Parse("#E5E5E5")),
            BorderThickness = new Thickness(0.5),
            Background = normalBackground,
            Child = sp,
            MinHeight = 60,
        };

        DragDrop.SetAllowDrop(border, true);

        border.AddHandler(DragDrop.DragOverEvent, (_, e) =>
        {
            var vm = DataContext as MainWindowViewModel;

            bool canMoveExisting = vm?.DraggedEvent is not null && cellDate.HasValue;
            bool canCreateNew = vm?.IsDraggingNewTask == true
                && cellDate.HasValue
                && !string.IsNullOrWhiteSpace(vm.NewTask.Title)
                && vm.NewTask.Category is not null;

            if (canMoveExisting)
                e.DragEffects = DragDropEffects.Move;
            else if (canCreateNew)
                e.DragEffects = DragDropEffects.Copy;
            else
                e.DragEffects = DragDropEffects.None;

            border.Background = (canMoveExisting || canCreateNew)
                ? hoverBackground
                : normalBackground;
        });

        border.AddHandler(DragDrop.DragLeaveEvent, (_, e) =>
        {
            border.Background = normalBackground;
        });

        border.AddHandler(DragDrop.DropEvent, (_, e) =>
        {
            border.Background = normalBackground;

            var vm = DataContext as MainWindowViewModel;
            if (vm is null || !cellDate.HasValue)
                return;

            if (vm.DraggedEvent is not null)
            {
                vm.RescheduleTask(vm.DraggedEvent, cellDate.Value);
                vm.DraggedEvent = null;
                SelectedDate = cellDate.Value;
                Rebuild();
                return;
            }

            if (vm.IsDraggingNewTask && !string.IsNullOrWhiteSpace(vm.NewTask.Title) && vm.NewTask.Category is not null)
            {
                vm.CreateTaskFromDraft(cellDate.Value);
                vm.IsDraggingNewTask = false;
                SelectedDate = cellDate.Value;
                Rebuild();
            }
        });

        if (cellDate.HasValue)
        {
            var captured = cellDate.Value;
            border.PointerPressed += (_, _) =>
            {
                var vm = DataContext as MainWindowViewModel;
                if (vm?.DraggedEvent is null)
                    SelectedDate = captured; 
            };
        }

        return border;
    }
}
