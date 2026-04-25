using System;
using System.Collections.Generic;
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

public partial class CategoryFormViewModel : ObservableObject 
{
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private Color selectedColor = Colors.LightBlue;
}

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<Event> Tasks { get; } = new();
    public ObservableCollection<EventViewModel> TaskViews { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public Array RepeatIntervals { get; } = Enum.GetValues(typeof(RepeatInterval));
    private static readonly TimeSpan RepeatLookahead = TimeSpan.FromDays(365);

    public IEnumerable<EventViewModel> RootTaskViews
    {
        get
        {
            var today = DateTime.Today;
            var horizon = today + RepeatLookahead;
            var result = new List<EventViewModel>();

            foreach (var vm in TaskViews.Where(v => !v.IsSubtask))
            {
                if (!vm.Event.IsRepeating)
                {
                    result.Add(vm);
                    continue;
                }

                var current = vm.Event.DueDate < today ? today : vm.Event.DueDate;
                while (current <= horizon)
                {
                    if (vm.Event.DueOn(current))
                        result.Add(new EventViewModel(vm.Event, Categories, current));
                    current = current.AddDays(1);
                }
            }

            return result.OrderBy(v => v.DueDate);
        }
    }
    public bool CanCancel => IsEditing || IsAddingSubtask;

    public EventViewModel? DraggedEvent { get; set; }
    public bool IsDraggingNewTask { get; set; }

    private bool canSave = true;
    private Guid? pendingParentId;
    private DateTime? _selectedOccurrenceDate;

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
            LinkSubtasks();
            SortTaskViews();
            NewTask.Category = Categories.FirstOrDefault();
            canSave = true;
            StorageService.Save(Tasks, Categories);
        }
    }

    public Color[] PresetColors =>
    [
        Colors.LightBlue, Colors.LightPink, Colors.LightGreen, Colors.LightSalmon,
        Colors.LightSeaGreen, Colors.LightSkyBlue, Colors.LightSteelBlue, Colors.LightYellow,
        Colors.Plum, Colors.PeachPuff, Colors.Thistle, Colors.Khaki,
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PanelTitle))]
    [NotifyPropertyChangedFor(nameof(PanelSaveLabel))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    private bool isAddingSubtask = false;

    [ObservableProperty] private TaskFormViewModel newTask = new();
    [ObservableProperty] private CategoryFormViewModel newCategory = new();
    [ObservableProperty] private DateTime selectedDate = DateTime.Today;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PanelTitle))]
    [NotifyPropertyChangedFor(nameof(PanelSaveLabel))]
    [NotifyPropertyChangedFor(nameof(IsEditing))]
    [NotifyPropertyChangedFor(nameof(ParentTaskTitle))]
    [NotifyPropertyChangedFor(nameof(HasParentTask))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyPropertyChangedFor(nameof(CanAddSubtask))]
    [NotifyPropertyChangedFor(nameof(ToggleCompleteLabel))]
    private EventViewModel? selectedTask;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTaskPanelVisible))]
    private bool isCategoryPanelOpen = false;

    public bool IsTaskPanelVisible => !IsCategoryPanelOpen;
    public bool IsEditing => SelectedTask is not null;
    public string PanelTitle => IsAddingSubtask ? "Add Subtask" : IsEditing ? "Edit Task" : "New Task";
    public string PanelSaveLabel => IsAddingSubtask ? "Save Subtask" : IsEditing ? "Update Task" : "Save Task";
    public string ToggleCompleteLabel => SelectedTask?.Event.IsComplete == true ? "Mark as Incomplete" : "Mark as Done";
    public string ParentTaskTitle
    {
        get
        {
            var parentId = SelectedTask?.Event.ParentId ?? pendingParentId;
            return parentId.HasValue
                ? Tasks.FirstOrDefault(t => t.Id == parentId.Value)?.Title ?? string.Empty
                : string.Empty;
        }
    }
    public bool HasParentTask => !string.IsNullOrWhiteSpace(ParentTaskTitle);

    public bool CanAddSubtask => IsEditing && SelectedTask is not null && !SelectedTask.IsSubtask;

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

        LinkSubtasks();
        SortTaskViews();
        NewTask.Category = Categories.FirstOrDefault();
    }

    private void LinkSubtasks()
    {
        foreach (var view in TaskViews)
            view.Subtasks.Clear();

        foreach (var child in Tasks.Where(t => t.ParentId.HasValue))
        {
            var parentId = child.ParentId;
            if (!parentId.HasValue) continue;

            var parent = TaskViews.FirstOrDefault(v => v.Event.Id == parentId.Value);
            var childVm = TaskViews.FirstOrDefault(v => v.Event.Id == child.Id);
            if (childVm is not null) parent?.Subtasks.Add(childVm);
        }

        OnPropertyChanged(nameof(RootTaskViews));
    }

    private void OnDataChanged(object? sender, NotifyCollectionChangedEventArgs _)
    {
        if (canSave)
        {
            StorageService.Save(Tasks, Categories);
        }
    }

    [RelayCommand]
    private void OpenCategoryPanel()
    {
        SelectedTask = null;
        pendingParentId = null;
        IsAddingSubtask = false;
        NewTask = new TaskFormViewModel { Category = Categories.FirstOrDefault() };
        NewCategory = new CategoryFormViewModel();
        IsCategoryPanelOpen = true;
    }

    [RelayCommand]
    private void CloseCategoryPanel()
    {
        IsCategoryPanelOpen = false;
        NewCategory = new CategoryFormViewModel();
    }

    [RelayCommand]
    private void AddCategory()
    {
        if (string.IsNullOrWhiteSpace(NewCategory.Name)) return;
        Categories.Add(new Category(NewCategory.Name.Trim(), NewCategory.SelectedColor));
        NewCategory = new CategoryFormViewModel();
    }

    [RelayCommand]
    private void DeleteCategory(Category category)
    {
        if (Categories.Count <= 1) return;

        var fallback = Categories.FirstOrDefault(c => c.Id != category.Id);
        foreach (var t in Tasks.Where(t => t.CategoryId == category.Id).ToList())
        {
            int idx = Tasks.IndexOf(t);
            var replacement = new Event(
                Title: t.Title,
                Description: t.Description,
                DueDate: t.DueDate,
                CategoryId: fallback!.Id,
                ParentId: t.ParentId,
                IsComplete: t.IsComplete,
                RepeatInterval: t.RepeatInterval)
            { Id = t.Id };

            Tasks[idx] = replacement;
            var vmIdx = TaskViews.IndexOf(TaskViews.First(v => v.Event.Id == t.Id));
            TaskViews[vmIdx] = new EventViewModel(replacement, Categories);
        }

        Categories.Remove(category);

        if (NewTask.Category?.Id == category.Id)
            NewTask.Category = Categories.FirstOrDefault();
    }

    [RelayCommand]
    private void SelectTask(EventViewModel evm)
    {
        pendingParentId = null;
        IsAddingSubtask = false;
        IsCategoryPanelOpen = false;
        SelectedTask = evm;
        _selectedOccurrenceDate = evm.OccurrenceDate;
        OnPropertyChanged(nameof(ParentTaskTitle));
        OnPropertyChanged(nameof(HasParentTask));
        var cat = Categories.FirstOrDefault(c => c.Id == evm.Event.CategoryId);
        NewTask = new TaskFormViewModel
        {
            Title = evm.Event.Title,
            Description = evm.Event.Description,
            DueDate = new DateTimeOffset(evm.OccurrenceDate, TimeZoneInfo.Local.GetUtcOffset(evm.OccurrenceDate)),
            RepeatInterval = evm.Event.RepeatInterval,
            Category = cat,
        };
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedTask = null;
        _selectedOccurrenceDate = null;
        pendingParentId = null;
        IsAddingSubtask = false;
        DraggedEvent = null;
        IsDraggingNewTask = false;
        NewTask = new TaskFormViewModel { Category = Categories.FirstOrDefault() };
        OnPropertyChanged(nameof(ParentTaskTitle));
        OnPropertyChanged(nameof(HasParentTask));
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
            { Id = old.Id };

            int idx = Tasks.IndexOf(old);
            if (idx >= 0) Tasks[idx] = updated;

            var existingVm = TaskViews.FirstOrDefault(v => v.Event.Id == old.Id);
            int vmIdx = existingVm is not null ? TaskViews.IndexOf(existingVm) : -1;
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
                ParentId: pendingParentId,
                RepeatInterval: NewTask.RepeatInterval);

            Tasks.Add(ev);
            TaskViews.Add(new EventViewModel(ev, Categories));
            pendingParentId = null;
            IsAddingSubtask = false;
        }

        LinkSubtasks();
        SortTaskViews();
        NewTask = new TaskFormViewModel { Category = Categories.FirstOrDefault() };
        SelectedTask = null;
        OnPropertyChanged(nameof(ParentTaskTitle));
        OnPropertyChanged(nameof(HasParentTask));
    }

    [RelayCommand]
    private void DeleteTask()
    {
        if (SelectedTask is null) return;

        var targetId = SelectedTask.Event.Id;
        var toRemove = new List<Event> { SelectedTask.Event };
        for (int i = 0; i < toRemove.Count; i++)
        {
            var parentId = toRemove[i].Id;
            toRemove.AddRange(Tasks.Where(t => t.ParentId == parentId && !toRemove.Any(r => r.Id == t.Id)));
        }

        foreach (var t in toRemove)
        {
            Tasks.Remove(t);
            var vm = TaskViews.FirstOrDefault(v => v.Event.Id == t.Id);
            if (vm is not null) TaskViews.Remove(vm);
        }

        LinkSubtasks();
        StorageService.Save(Tasks, Categories);
        SelectedTask = null;
        NewTask = new TaskFormViewModel { Category = Categories.FirstOrDefault() };
    }

    [RelayCommand]
    private void AddSubtask()
    {
        if (SelectedTask is null)
            return;

        if (SelectedTask.IsSubtask)
            return;

        pendingParentId = SelectedTask.Event.Id;
        IsAddingSubtask = true;
        IsCategoryPanelOpen = false;
        NewTask = new TaskFormViewModel
        {
            Category = Categories.FirstOrDefault(c => c.Id == SelectedTask.Event.CategoryId),
            DueDate = new DateTimeOffset(SelectedTask.OccurrenceDate, TimeZoneInfo.Local.GetUtcOffset(SelectedTask.OccurrenceDate)),
        };

        SelectedTask = null;
    }

    [RelayCommand]
    private void ToggleComplete()
    {
        if (SelectedTask is null) return;

        var old = SelectedTask.Event;
        var occurrenceDate = _selectedOccurrenceDate ?? SelectedTask.OccurrenceDate;

        Event updated;

        if (old.IsRepeating && !old.IsComplete)
        {
            var completedCopy = new Event(
                Title: old.Title,
                Description: old.Description,
                DueDate: occurrenceDate,
                CategoryId: old.CategoryId,
                ParentId: old.ParentId,
                IsComplete: true,
                RepeatInterval: RepeatInterval.None)
            { Id = Guid.NewGuid() };

            Tasks.Add(completedCopy);
            TaskViews.Add(new EventViewModel(completedCopy, Categories));

            var nextDate = occurrenceDate.AddDays(old.RepeatInterval switch
            {
                RepeatInterval.Daily => 1,
                RepeatInterval.Weekly => 7,
                RepeatInterval.Monthly => DateTime.DaysInMonth(occurrenceDate.Year, occurrenceDate.Month),
                RepeatInterval.Yearly => (occurrenceDate.AddYears(1) - occurrenceDate).Days,
                _ => 1
            });

            var advancedSeries = new Event(
                Title: old.Title,
                Description: old.Description,
                DueDate: nextDate,
                CategoryId: old.CategoryId,
                ParentId: old.ParentId,
                IsComplete: false,
                RepeatInterval: old.RepeatInterval)
            { Id = old.Id };

            int idx = Tasks.IndexOf(old);
            if (idx >= 0) Tasks[idx] = advancedSeries;

            var existingVm = TaskViews.FirstOrDefault(v => v.Event.Id == old.Id);
            int vmIdx = existingVm is not null ? TaskViews.IndexOf(existingVm) : -1;
            if (vmIdx >= 0) TaskViews[vmIdx] = new EventViewModel(advancedSeries, Categories);

            updated = completedCopy;
        }
        else
        {
            updated = new Event(
                Title: old.Title,
                Description: old.Description,
                DueDate: occurrenceDate,
                CategoryId: old.CategoryId,
                ParentId: old.ParentId,
                IsComplete: !old.IsComplete,
                RepeatInterval: old.RepeatInterval)
            { Id = old.Id };

            int idx = Tasks.IndexOf(old);
            if (idx >= 0) Tasks[idx] = updated;

            var existingVm = TaskViews.FirstOrDefault(v => v.Event.Id == old.Id);
            int vmIdx = existingVm is not null ? TaskViews.IndexOf(existingVm) : -1;
            if (vmIdx >= 0) TaskViews[vmIdx] = new EventViewModel(updated, Categories);
        }

        LinkSubtasks();
        StorageService.Save(Tasks, Categories);

        SelectedTask = TaskViews.FirstOrDefault(v => v.Event.Id == updated.Id);
        _selectedOccurrenceDate = SelectedTask?.OccurrenceDate;
        OnPropertyChanged(nameof(RootTaskViews));
        OnPropertyChanged(nameof(ParentTaskTitle));
        OnPropertyChanged(nameof(HasParentTask));
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

    public void RescheduleTask(EventViewModel evm, DateTime newDueDate)
    {
        var old = evm.Event;
        var updated = new Event(
            Title: old.Title,
            Description: old.Description,
            DueDate: newDueDate,
            CategoryId: old.CategoryId,
            ParentId: old.ParentId,
            IsComplete: old.IsComplete,
            RepeatInterval: old.RepeatInterval)
        { Id = old.Id };

        int taskIdx = Tasks.IndexOf(old);
        if (taskIdx >= 0)
            Tasks[taskIdx] = updated;

        var existingVm = TaskViews.FirstOrDefault(v => v.Event.Id == old.Id);
        int vmIdx = existingVm is not null ? TaskViews.IndexOf(existingVm) : -1; 
        if (vmIdx >= 0)
            TaskViews[vmIdx] = new EventViewModel(updated, Categories);

        if (SelectedTask?.Event.Id == old.Id)
        {
            SelectedTask = TaskViews.FirstOrDefault(v => v.Event.Id == updated.Id);
            _selectedOccurrenceDate = newDueDate;
        } 

        LinkSubtasks();
        SortTaskViews();
        StorageService.Save(Tasks, Categories);
    }

    public void CreateTaskFromDraft(DateTime dueDate)
    {
        if (string.IsNullOrWhiteSpace(NewTask.Title) || NewTask.Category is null)
            return;

        var ev = new Event(
            Title: NewTask.Title,
            Description: NewTask.Description,
            DueDate: dueDate,
            CategoryId: NewTask.Category.Id,
            ParentId: pendingParentId,
            RepeatInterval: NewTask.RepeatInterval);

        Tasks.Add(ev);
        TaskViews.Add(new EventViewModel(ev, Categories));

        SelectedTask = null;
        pendingParentId = null;
        IsAddingSubtask = false;
        DraggedEvent = null;
        IsDraggingNewTask = false;

        LinkSubtasks();
        SortTaskViews();
        StorageService.Save(Tasks, Categories);

        NewTask = new TaskFormViewModel { Category = Categories.FirstOrDefault() };
        OnPropertyChanged(nameof(ParentTaskTitle));
        OnPropertyChanged(nameof(HasParentTask));
    }

    public void BeginDraftTask()
    {
        SelectedTask = null;
        DraggedEvent = null;
        IsDraggingNewTask = false; 

        OnPropertyChanged(nameof(ParentTaskTitle));
        OnPropertyChanged(nameof(HasParentTask));
    }
}