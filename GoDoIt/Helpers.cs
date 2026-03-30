using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
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
    public static readonly StyledProperty<ObservableCollection<EventViewModel>?> TasksProperty =
        AvaloniaProperty.Register<CalendarView, ObservableCollection<EventViewModel>?>(nameof(Tasks));

    public static readonly StyledProperty<DateTime> SelectedDateProperty =
        AvaloniaProperty.Register<CalendarView, DateTime>(nameof(SelectedDate), DateTime.Today);

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
                var cellDate = isCurrentMonth ? new DateTime(date.Year, date.Month, dayNum) : (DateTime?)null;

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
                var chip = new Border
                {
                    Background = new SolidColorBrush(evm.Color), 
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(4, 1),
                    Margin = new Thickness(2, 1),
                    Child = new TextBlock
                    {
                        Text = evm.Title,
                        FontSize = 10,
                        Foreground = Brushes.White,
                        TextWrapping = TextWrapping.NoWrap
                    }
                };
                sp.Children.Add(chip);
            }
        }

        var border = new Border
        {
            BorderBrush = new SolidColorBrush(Color.Parse("#E5E5E5")),
            BorderThickness = new Thickness(0.5),
            Background = isSelected
                ? new SolidColorBrush(Color.Parse("#EDE7FF"))
                : Brushes.Transparent,
            Child = sp,
            MinHeight = 60
        };

        if (cellDate.HasValue)
        {
            var captured = cellDate.Value;
            border.PointerPressed += (_, _) => SelectedDate = captured;
        }

        return border;
    }
}
