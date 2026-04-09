using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.Platform.Storage;
using Avalonia.Media;
using Avalonia;
using GoDoIt;
using GoDoIt.ViewModels;
using Ical.Net.Serialization;
using System.Linq;

namespace GoDoIt.Views;

public partial class MainWindow : Window
{
    private EventViewModel? _draggedEvent;
    private Point _dragStartPoint;

    public MainWindow()
    {
        InitializeComponent();

        calendar.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        calendar.AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnTaskCardPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border { Tag: EventViewModel evm } &&
            DataContext is MainWindowViewModel vm)
        {
            vm.SelectTaskCommand.Execute(evm);
        }
    }

    private async void OnTaskCardPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_draggedEvent is null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        var currentPos = e.GetPosition(this);

        if (Math.Abs(currentPos.X - _dragStartPoint.X) < 5 &&
            Math.Abs(currentPos.Y - _dragStartPoint.Y) < 5)
            return;

        await DragDrop.DoDragDropAsync(e, new DataTransfer(), DragDropEffects.Move);
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
        e.DragEffects = _draggedEvent is not null
            ? DragDropEffects.Move
            : DragDropEffects.None;
    }
}
