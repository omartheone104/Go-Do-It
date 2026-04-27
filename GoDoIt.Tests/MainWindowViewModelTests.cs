using NUnit.Framework;
using System.IO;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GoDoIt;
using GoDoIt.ViewModels;
using GoDoIt.Views;

namespace GoDoIt.Tests;

[TestFixture]
public class MainWindowViewModelTests
{
    private string _tempData = "";
    private string _tempBackup = "";

    [OneTimeSetUp]
    public void StashUserData()
    {
        _tempData   = Path.GetTempFileName();
        _tempBackup = Path.GetTempFileName();

        if (File.Exists(StorageService.DataFile))
            File.Move(StorageService.DataFile, _tempData, overwrite: true);
        if (File.Exists(StorageService.BackupFile))
            File.Move(StorageService.BackupFile, _tempBackup, overwrite: true);
    }

    [OneTimeTearDown]
    public void RestoreUserData()
    {
        File.Move(_tempData, StorageService.DataFile, overwrite: true);
        File.Move(_tempBackup, StorageService.BackupFile, overwrite: true);

        File.Delete(_tempData);
        File.Delete(_tempBackup);
    }

    [AvaloniaTest]
    public void Check_Calendar()
    {
        var window = new MainWindow
        {
            DataContext = new MainWindowViewModel(loadFromDisk: false)
        };

        var date = new System.DateTime(2026, 3, 18);
        window.Show();
        Assert.That(window.Find<Avalonia.Controls.Calendar>("calendar"), Is.Not.Null);
        window.Find<Avalonia.Controls.Calendar>("calendar")!.SelectedDate = date;
        Assert.That(window.Find<TextBlock>("calendar_text"), Is.Not.Null);
        Assert.That(window.Find<TextBlock>("calendar_text")!.Text, Is.EqualTo(date.ToString()));
    }

    [Test]
    public void Constructor_CategoriesInitialized()
    {
        var vm = new MainWindowViewModel(loadFromDisk: false);
        Assert.That(vm.Categories.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Constructor_TasksEmpty()
    {
        var vm = new MainWindowViewModel(loadFromDisk: false);
        Assert.That(vm.Tasks.Count, Is.EqualTo(0));
    }

    [Test]
    public void SaveTask_AddsToTasksAndTaskViews()
    {
        var vm = new MainWindowViewModel(loadFromDisk: false);
        vm.NewTask.Title = "New Task";
        vm.NewTask.Description = "Test";
        vm.NewTask.Category = vm.Categories[0];
        vm.SaveTaskCommand.Execute(null);
        Assert.That(vm.Tasks.Count, Is.EqualTo(1));
        Assert.That(vm.TaskViews.Count, Is.EqualTo(1));
    }

    [Test]
    public void SaveTask_DoesNothing_WhenTitleEmpty()
    {
        var vm = new MainWindowViewModel(loadFromDisk: false);
        vm.NewTask.Title = "";
        vm.NewTask.Category = vm.Categories[0];
        vm.SaveTaskCommand.Execute(null);
        Assert.That(vm.Tasks.Count, Is.EqualTo(0));
    }

    [Test]
    public void SaveTask_DoesNothing_WhenCategoryNull()
    {
        var vm = new MainWindowViewModel(loadFromDisk: false);
        vm.NewTask.Title = "Test";
        vm.NewTask.Category = null;
        vm.SaveTaskCommand.Execute(null);
        Assert.That(vm.Tasks.Count, Is.EqualTo(0));
    }

    [Test]
    public void SaveTask_ResetsNewTask_AfterSaving()
    {
        var vm = new MainWindowViewModel(loadFromDisk: false);
        vm.NewTask.Title = "New Task";
        vm.NewTask.Category = vm.Categories[0];
        vm.SaveTaskCommand.Execute(null);
        Assert.That(vm.NewTask.Title, Is.EqualTo(string.Empty));
    }

    [Test]
    public void SaveTask_TaskViewColorMatchesCategory()
    {
        var vm = new MainWindowViewModel(loadFromDisk: false);
        vm.NewTask.Title = "Test";
        vm.NewTask.Category = vm.Categories[0];
        vm.SaveTaskCommand.Execute(null);
        Assert.That(vm.TaskViews[0].Color, Is.EqualTo(vm.Categories[0].Color));
    }
}
