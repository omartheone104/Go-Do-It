using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

    [ObservableProperty] private TaskFormViewModel newTask = new();
    [ObservableProperty] private DateTime selectedDate = DateTime.Today;

    public MainWindowViewModel() : this(loadFromDisk: true) { }

    public MainWindowViewModel(bool loadFromDisk)
    {
        if (loadFromDisk)
        {
            LoadFromDisk();
        }
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
        {
            foreach (var c in categories)
                Categories.Add(c);
        }
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
        => StorageService.Save(Tasks, Categories);

    [RelayCommand]
    private void SaveTask()
    {
        if (string.IsNullOrWhiteSpace(NewTask.Title) || NewTask.Category is null)
            return;

        var due = NewTask.DueDate.HasValue
            ? NewTask.DueDate.Value.Date
            : DateTime.Today;

        var ev = new Event(
            Title: NewTask.Title,
            Description: NewTask.Description,
            DueDate: due,
            CategoryId: NewTask.Category.Id,
            RepeatInterval: NewTask.RepeatInterval);

        Tasks.Add(ev);
        TaskViews.Add(new EventViewModel(ev, Categories));
        SortTaskViews();

        NewTask = new TaskFormViewModel { Category = Categories.FirstOrDefault() };
    }

    private void SortTaskViews()
    {
        var sorted = TaskViews.OrderBy(t => t.DueDate).ToList();
        TaskViews.Clear();
        foreach (var t in sorted)
            TaskViews.Add(t);
    }
}
