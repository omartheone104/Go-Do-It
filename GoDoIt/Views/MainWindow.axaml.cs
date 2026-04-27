using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.Platform.Storage;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia;
using GoDoIt;
using GoDoIt.ViewModels;
using Ical.Net.Serialization;
using System.Linq;
using System.Text.RegularExpressions;
namespace GoDoIt.Views;

public partial class MainWindow : Window
{
    private Point _dragStartPoint;
    private Point _draftDragStartPoint;

    public MainWindow()
    {
        InitializeComponent();

        calendar.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        calendar.AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void SetLightTheme(object? sender, RoutedEventArgs e)
    {
        ApplyLightTheme();
    }

    private void ApplyLightTheme()
    {
        var app = Application.Current!;

        app.RequestedThemeVariant = ThemeVariant.Light;

        app.Resources.Remove("ThemeBackgroundBrush");
        app.Resources.Remove("ThemeForegroundBrush");
        app.Resources.Remove("SystemControlBackgroundAltHighBrush");
        app.Resources.Remove("SystemControlBackgroundChromeMediumLowBrush");
        app.Resources.Remove("AccentFillColorDefaultBrush");
        app.Resources.Remove("SystemControlForegroundBaseHighBrush");
        app.Resources.Remove("SystemControlForegroundChromeHighBrush");
        app.Resources.Remove("ThemeMutedBrush");
        app.Resources.Remove("CalendarBorderBrush");
        app.Resources.Remove("CalendarSelectedBrush");
    }

    private void SetDarkTheme(object? sender, RoutedEventArgs e)
    {
        ApplyDarkTheme();
    }

    private void ApplyDarkTheme()
    {
        var app = Application.Current!;

        app.RequestedThemeVariant = ThemeVariant.Dark;

        app.Resources["ThemeBackgroundBrush"] = new SolidColorBrush(Color.Parse("#1E1E1E"));
        app.Resources["ThemeForegroundBrush"] = new SolidColorBrush(Color.Parse("#F5F5F5"));
        app.Resources["SystemControlBackgroundAltHighBrush"] = new SolidColorBrush(Color.Parse("#2B2B2B"));
        app.Resources["SystemControlBackgroundChromeMediumLowBrush"] = new SolidColorBrush(Color.Parse("#333333"));
        app.Resources["AccentFillColorDefaultBrush"] = new SolidColorBrush(Color.Parse("#7B67C8"));
        app.Resources["SystemControlForegroundBaseHighBrush"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
        app.Resources["SystemControlForegroundChromeHighBrush"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
        app.Resources["ThemeMutedBrush"] = new SolidColorBrush(Color.Parse("#9A9A9A"));
        app.Resources["CalendarBorderBrush"] = new SolidColorBrush(Color.Parse("#444444"));
        app.Resources["CalendarSelectedBrush"] = new SolidColorBrush(Color.Parse("#3A2F5C"));
    }

    private void OnTaskCardPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border { Tag: EventViewModel evm } &&
            DataContext is MainWindowViewModel vm)
        {
            vm.IsDraggingNewTask = false;
            vm.SelectTaskCommand.Execute(evm);
            vm.DraggedEvent = evm;
            _dragStartPoint = e.GetPosition(this);
        }
    }

    private async void OnTaskCardPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm || vm.DraggedEvent is null)
            return;

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        var currentPos = e.GetPosition(this);

        if (Math.Abs(currentPos.X - _dragStartPoint.X) < 5 &&
            Math.Abs(currentPos.Y - _dragStartPoint.Y) < 5)
            return;

        if (sender is Border border)
            border.Opacity = 0.5;

        await DragDrop.DoDragDropAsync(e, new DataTransfer(), DragDropEffects.Move);

        if (sender is Border borderAfter)
            borderAfter.Opacity = 1.0;
    }

    public async void Export_Events(object sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Calendar",
            DefaultExtension = ".ics",
            ShowOverwritePrompt = true,
            FileTypeChoices = [new FilePickerFileType("iCalendar Files") {
                Patterns = ["*.ical", "*.ics", "*.ifb", "*.icalendar"],
                MimeTypes = ["text/calendar"],
                AppleUniformTypeIdentifiers = ["iCal", "iFBf"]
            }]
        });

        if (file is not null && DataContext is MainWindowViewModel vm)
        {
            await using var stream = await file.OpenWriteAsync();
            using var writer = new StreamWriter(stream);

            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(vm.Calendar);
            writer.WriteLine(serializedCalendar);
        }
    }

    public async void Import_Events(object sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import Calendar",
            AllowMultiple = false,

            FileTypeFilter = [new FilePickerFileType("iCalendar Files") {
                Patterns = ["*.ical", "*.ics", "*.ifb", "*.icalendar"],
                MimeTypes = ["text/calendar"],
                AppleUniformTypeIdentifiers = ["iCal", "iFBf"]
            }]
        });


        if (files.Count > 0 && files[0] is IStorageFile file && DataContext is MainWindowViewModel vm)
        {
            await using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);

            if (Ical.Net.Calendar.Load(reader) is Ical.Net.Calendar cal)
            {
                vm.Calendar = cal;
            }
        }
    }

    private void OnDeleteCategoryClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button { Tag: Category cat } &&
            DataContext is MainWindowViewModel vm)
        {
            vm.DeleteCategoryCommand.Execute(cat);
        }
    }

    private void OnColorSwatchPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border { Tag: Color color } &&
            DataContext is MainWindowViewModel vm)
        {
            vm.NewCategory.SelectedColor = color;
        }
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        var visualTarget = calendar.InputHitTest(e.GetPosition(calendar));
        
        var dayButton = visualTarget as Control;
        while (dayButton != null && dayButton.DataContext is not DateTime)
        {
            dayButton = dayButton.Parent as Control;
        }

        if (dayButton?.DataContext is DateTime droppedDate)
        {
            Console.WriteLine($"Item dropped on: {droppedDate.ToShortDateString()}");
        }
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        if (vm.DraggedEvent is not null)
            e.DragEffects = DragDropEffects.Move;
        else if (vm.IsDraggingNewTask)
            e.DragEffects = DragDropEffects.Copy;
        else
            e.DragEffects = DragDropEffects.None;
    }

    private void OnDraftTaskPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        if (string.IsNullOrWhiteSpace(vm.NewTask.Title))
            return;

        vm.BeginDraftTask();
        vm.DraggedEvent = null;
        vm.IsDraggingNewTask = true;
        _draftDragStartPoint = e.GetPosition(this);
    }

    private async void OnDraftTaskPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm || !vm.IsDraggingNewTask)
            return;

        if (vm.IsEditing)
            return;

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        var currentPos = e.GetPosition(this);

        if (Math.Abs(currentPos.X - _draftDragStartPoint.X) < 5 &&
            Math.Abs(currentPos.Y - _draftDragStartPoint.Y) < 5)
            return;
        
        if (sender is Control control)
            control.Opacity = 0.6;

        await DragDrop.DoDragDropAsync(e, new DataTransfer(), DragDropEffects.Copy);

        if (sender is Control controlAfter)
            controlAfter.Opacity = 1.0;

        vm.DraggedEvent = null;
        vm.IsDraggingNewTask = false;
    }
}
