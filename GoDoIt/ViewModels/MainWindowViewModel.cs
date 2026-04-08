using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ical.Net;

namespace GoDoIt.ViewModels;

public partial class TaskFormViewModel : ObservableObject
{
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private DateTimeOffset? dueDate = DateTimeOffset.Now;
    [ObservableProperty] private RepeatInterval repeatInterval = RepeatInterval.None;
    [ObservableProperty] private Category? category;
}

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<Event> Tasks { get; } = new();
    public ObservableCollection<EventViewModel> TaskViews { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public Array RepeatIntervals { get; } = Enum.GetValues(typeof(RepeatInterval));

    private bool canSave = true;
    public Ical.Net.Calendar Calendar
    {
        get
        {
            Ical.Net.Calendar cal = new();
            cal.Events.AddRange(Tasks.Select(t => t.AsCalendarEvent()));
            foreach (var category in Categories)
            {
                cal.AddProperty(category.AsCalendarProperty());
            }
            return cal;
        }
        set
        {
            canSave = false;
            Categories.Clear();
            Tasks.Clear();
            TaskViews.Clear();
            foreach (var category in value.Properties.Where(p => p.Name == Category.PROPERTY_NAME).Select(Category.FromCalendarProperty).OfType<Category>())
            {
                Categories.Add(category);
            }

            foreach (Event task in value.Events.Select(Event.FromCalendarEvent))
            {
                Tasks.Add(task);
                TaskViews.Add(new EventViewModel(task, Categories));
            }
            SortTaskViews();
            NewTask.Category = Categories.FirstOrDefault();
            canSave = true;
            StorageService.Save(Tasks, Categories);
        }
    }

    [ObservableProperty] private TaskFormViewModel newTask = new();
    [ObservableProperty] private DateTime selectedDate = DateTime.Today;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PanelTitle))]
    [NotifyPropertyChangedFor(nameof(PanelSaveLabel))]
    [NotifyPropertyChangedFor(nameof(IsEditing))]
    private EventViewModel? selectedTask;

    public bool IsEditing => SelectedTask is not null;
    public string PanelTitle => IsEditing ? "Edit Task" : "New Task";
    public string PanelSaveLabel => IsEditing ? "Update Task" : "Save Task";

    public MainWindowViewModel() : this(loadFromDisk: true) { }

    public MainWindowViewModel(bool loadFromDisk)
    {
        if (loadFromDisk)
            LoadFromDisk();
        else
        {
            Categories.Add(new Category("Homework", Colors.LightBlue));
            Categories.Add(new Category("Career", Colors.LightPink));
            NewTask.Category = Categories.FirstOrDefault();
        }

        Tasks.CollectionChanged += OnDataChanged;
        Categories.CollectionChanged += OnDataChanged;
    }

    private void LoadFromDisk()
    {
        var (tasks, categories) = StorageService.Load();

        if (categories.Count > 0)
            foreach (var c in categories) Categories.Add(c);
        else
        {
            Categories.Add(new Category("Homework", Colors.LightBlue));
            Categories.Add(new Category("Career", Colors.LightPink));
        }

        foreach (var t in tasks)
        {
            Tasks.Add(t);
            TaskViews.Add(new EventViewModel(t, Categories));
        }

        SortTaskViews();
        NewTask.Category = Categories.FirstOrDefault();
    }

    private void OnDataChanged(object? sender, NotifyCollectionChangedEventArgs _)
    {
        if (canSave)
        {
            StorageService.Save(Tasks, Categories);
        }
    }

    [RelayCommand]
    private void SelectTask(EventViewModel evm)
    {
        SelectedTask = evm;
        var cat = Categories.FirstOrDefault(c => c.Id == evm.Event.CategoryId);
        NewTask = new TaskFormViewModel
        {
            Title = evm.Event.Title,
            Description = evm.Event.Description,
            DueDate = new DateTimeOffset(evm.Event.DueDate, TimeZoneInfo.Local.GetUtcOffset(evm.Event.DueDate)),
            RepeatInterval = evm.Event.RepeatInterval,
            Category = cat,
        };
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedTask = null;
        NewTask = new TaskFormViewModel { Category = Categories.FirstOrDefault() };
    }

    [RelayCommand]
    private void SaveTask()
    {
        if (string.IsNullOrWhiteSpace(NewTask.Title) || NewTask.Category is null)
            return;

        var due = NewTask.DueDate.HasValue
            ? NewTask.DueDate.Value.Date
            : DateTime.Today;

        if (IsEditing)
        {
            var old = SelectedTask!.Event;
            var updated = new Event(
                Title: NewTask.Title,
                Description: NewTask.Description,
                DueDate: due,
                CategoryId: NewTask.Category.Id,
                ParentId: old.ParentId,
                IsComplete: old.IsComplete,
                RepeatInterval: NewTask.RepeatInterval)
            {
                Id = old.Id
            };

            int idx = Tasks.IndexOf(old);
            if (idx >= 0) Tasks[idx] = updated;

            var vmIdx = TaskViews.IndexOf(SelectedTask);
            if (vmIdx >= 0) TaskViews[vmIdx] = new EventViewModel(updated, Categories);

            StorageService.Save(Tasks, Categories);
            SelectedTask = null;
        }
        else
        {
            var ev = new Event(
                Title: NewTask.Title,
                Description: NewTask.Description,
                DueDate: due,
                CategoryId: NewTask.Category.Id,
                RepeatInterval: NewTask.RepeatInterval);

            Tasks.Add(ev);
            TaskViews.Add(new EventViewModel(ev, Categories));
        }

        SortTaskViews();
        NewTask = new TaskFormViewModel { Category = Categories.FirstOrDefault() };
    }

    [RelayCommand]
    private void DeleteTask()
    {
        if (SelectedTask is null) return;

        var targetId = SelectedTask.Event.Id;
        var toRemove = Tasks
            .Where(t => t.Id == targetId || t.ParentId == targetId)
            .ToList();

        foreach (var t in toRemove)
        {
            Tasks.Remove(t);
            var vm = TaskViews.FirstOrDefault(v => v.Event.Id == t.Id);
            if (vm is not null) TaskViews.Remove(vm);
        }

        StorageService.Save(Tasks, Categories);
        SelectedTask = null;
        NewTask = new TaskFormViewModel { Category = Categories.FirstOrDefault() };
    }

    private void SortTaskViews()
    {
        var sorted = TaskViews.OrderBy(t => t.DueDate).ToList();
        TaskViews.Clear();
        foreach (var t in sorted) TaskViews.Add(t);
    }

    [RelayCommand]
    private static void ExitApp() => Environment.Exit(0);

    [RelayCommand]
    private static void AboutApp()
    {
        // https://stackoverflow.com/a/43232486
        var url = @"https://github.com/omartheone104/Go-Do-It";
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }

}
