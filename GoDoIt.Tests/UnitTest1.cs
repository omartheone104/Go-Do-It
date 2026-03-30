using NUnit.Framework;
using NUnit.Framework.Legacy;
using Avalonia.Headless.NUnit;
using Avalonia.Controls;
using GoDoIt.Views;
using GoDoIt.ViewModels;
using System;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Ical.Net.DataTypes;


namespace GoDoIt.Tests
{
    [TestFixture]
    public class BasicTests
    {
        [Test]
        public void SaveAndLoad_String_Works()
        {
            using var stream = new MemoryStream();
            var data = Encoding.UTF8.GetBytes("Go Do It Test");
            stream.Write(data, 0, data.Length);

            stream.Position = 0;
            var loaded = new byte[stream.Length];
            stream.Read(loaded, 0, loaded.Length);

            string result = Encoding.UTF8.GetString(loaded);
            Assert.That(result, Is.EqualTo("Go Do It Test"));
        }

        [AvaloniaTest]
        public void Check_Calendar()
        {
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

            // Show the window, as it's required to get layout processed:
            var date = new DateTime(2026, 3, 18, 00, 00, 00, 000);
            window.Show();
            Assert.That(window.Find<Avalonia.Controls.Calendar>("calendar"), Is.Not.Null);
            window.Find<Avalonia.Controls.Calendar>("calendar").SelectedDate = date;
            Assert.That(window.Find<TextBlock>("calendar_text"), Is.Not.Null);
            Assert.That(window.Find<TextBlock>("calendar_text").Text, Is.EqualTo(date.ToString()));
        }

    }

    [TestFixture]
    public class EventTests
    {
        [Test]
        public void DueToday_WhenDateIsToday_ReturnsTrue()
        {
            var TodayEvent = new Event(
                Title: "Today",
                Description: "Today Test Event",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(TodayEvent.DueToday(), Is.True);
        }

        [Test]
        public void DueToday_WhenDateIsFuture_ReturnsFalse()
        {
            var FutureEvent = new Event(
                Title: "Future",
                Description: "Future Test Event",
                DueDate: DateTime.Today.AddDays(10),
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(FutureEvent.DueToday(), Is.False);
        }

        [Test]
        public void DueToday_WhenDateIsPast_ReturnsFalse()
        {
            var PastEvent = new Event(
                Title: "Past",
                Description: "Past Test Event",
                DueDate: DateTime.Today.AddDays(-10),
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(PastEvent.DueToday(), Is.False);
        }

        [Test]
        public void CompletedEvent_IsNotDueToday()
        {
            var completedEvent = new Event(
                Title: "Completed Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: true,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(completedEvent.DueToday(), Is.False);
        }

        [Test]
        public void Event_CategoryUUIDPreserved()
        {
            var catId = new Guid();

            var @event = new Event(
                Title: "Test Event",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: catId,
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.CategoryId, Is.EqualTo(catId));
        }

        [Test]
        public void NextOccurrenceFrom_DueAfterCheckedDate_ReturnsFirstOccurrence()
        {
            var dueDate = new DateTime(2025, 1, 1);
            var checkedDate = dueDate.AddDays(-10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            );

            Assert.That(@event.NextOccurrenceFrom(checkedDate), Is.EqualTo(dueDate));
        }

        [Test]
        public void NextOccurrenceFrom_DueBeforeCheckedDate_WithOccuranceOnCheckedDate_ReturnsNextOccurance()
        {
            var dueDate = new DateTime(2025, 1, 1);
            var checkedDate = dueDate.AddDays(10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            );

            Assert.That(@event.NextOccurrenceFrom(checkedDate), Is.EqualTo(dueDate.AddDays(10)));
        }

        [Test]
        public void NextOccurrenceFrom_DueBeforeCheckedDate_WithoutOccuranceOnCheckedDate_ReturnsNextOccurance()
        {
            var dueDate = new DateTime(2025, 1, 1);
            var checkedDate = new DateTime(2025, 1, 2);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Monthly
            );

            Assert.That(@event.NextOccurrenceFrom(checkedDate), Is.EqualTo(new DateTime(2025, 2, 1)));
        }

        [Test]
        public void Event_YearlyRepeat_OnFebruary29_OnlyEveryFourYears()
        {
            var dueDate = new DateTime(2024, 2, 29);
            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Yearly
            );
            Assert.That(@event.NextOccurrenceFrom(new DateTime(2026, 2, 1)), Is.EqualTo(new DateTime(2028, 2, 29)));
        }

        [Test]
        public void NextOccurrenceFrom_WhenEventIsComplete_ReturnsNull()
        {
            var dueDate = new DateTime(2025, 1, 11);
            var checkedDate = dueDate.AddDays(10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: true,
                RepeatInterval: RepeatInterval.Daily
            );

            Assert.That(@event.NextOccurrenceFrom(checkedDate), Is.Null);
        }

        [Test]
        public void NextOccurrenceFrom_WhenNoRepeat_AndCheckedDateAfterDueDate_ReturnsNull()
        {

            var dueDate = new DateTime(2025, 1, 11);
            var checkedDate = dueDate.AddDays(10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrenceFrom(checkedDate), Is.Null);
        }

        [Test]
        public void NextOccurrenceFrom_WhenNoRepeat_AndCheckedDateBeforeDueDate_ReturnsDueDate()
        {

            var dueDate = new DateTime(2025, 1, 11);
            var checkedDate = dueDate.AddDays(-10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrenceFrom(checkedDate), Is.EqualTo(dueDate));
        }

        [Test]
        public void NextOccurrence_WhenDueToday_ReturnsToday()
        {
            var dueDate = DateTime.Today;

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrence(), Is.EqualTo(DateTime.Today));
        }

        [Test]
        public void NextOccurrence_WhenDueInFuture_ReturnsDueDate()
        {
            var dueDate = DateTime.Today.AddDays(10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrence(), Is.EqualTo(DateTime.Today.AddDays(10)));
        }

        [Test]
        public void NextOccurrence_WhenDueInPast_NoRepeat_ReturnsNull()
        {
            var dueDate = DateTime.Today.AddDays(-10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrence(), Is.Null);
        }

        [Test]
        public void NextOccurrence_WhenDueInPast_DailyRepeat_ReturnsToday()
        {
            var dueDate = DateTime.Today.AddDays(-10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            );

            Assert.That(@event.NextOccurrence(), Is.EqualTo(DateTime.Today));
        }

        [Test]
        public void NextOccurrence_WhenDueInPast_YearlyRepeat_ReturnsNextYear()
        {
            var dueDate = DateTime.Today.AddDays(-10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Yearly
            );

            Assert.That(@event.NextOccurrence(), Is.EqualTo(dueDate.AddYears(1)));
        }

        [Test]
        public void NextOccurrence_WhenDueInPast_AndWhenComplete_ReturnsNull()
        {
            var dueDate = DateTime.Today.AddDays(-10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: true,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrence(), Is.Null);
        }

        [Test]
        public void NextOccurrence_WhenDueToday_AndWhenComplete_ReturnsNull()
        {
            var dueDate = DateTime.Today;

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: true,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrence(), Is.Null);
        }

        [Test]
        public void NextOccurrence_WhenDueInFuture_AndWhenComplete_ReturnsNull()
        {
            var dueDate = DateTime.Today.AddDays(10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: true,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrence(), Is.Null);
        }

        [Test]
        public void Constructor_DefaultParentIsNull()
        {
            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.ParentId, Is.Null);
        }

        [Test]
        public void Constructor_DefaultIsCompleteFalse()
        {
            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                ParentId: null,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.IsComplete, Is.False);
        }

        [Test]
        public void Constructor_DefaultIsRepeatingFalse()
        {
            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false
            );

            Assert.That(@event.IsRepeating, Is.False);
        }

        [Test]
        public void Occurances_CountOne_WhenNonRepeat()
        {
            var dueDate = DateTime.Today;

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.Occurances.Count(), Is.EqualTo(1));
        }

        [Test]
        public void FirstOccuranceIsDueDate_WhenNoRepeat()
        {
            var dueDate = DateTime.Today;

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.Occurances.First(), Is.EqualTo(dueDate));
        }

        [Test]
        public void FirstOccuranceIsDueDate_WhenRepeat()
        {
            var dueDate = DateTime.Today;

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            );

            Assert.That(@event.Occurances.First(), Is.EqualTo(dueDate));
        }
    }

    [TestFixture]
    public class CategoryTests
    {
        [Test]
        public void Category_NameIsPreserved()
        {
            var category = new Category("Homework", Avalonia.Media.Colors.LightBlue);
            Assert.That(category.Name, Is.EqualTo("Homework"));
        }
 
        [Test]
        public void Category_ColorIsPreserved()
        {
            var category = new Category("Homework", Avalonia.Media.Colors.LightBlue);
            Assert.That(category.Color, Is.EqualTo(Avalonia.Media.Colors.LightBlue));
        }
 
        [Test]
        public void Category_TwoWithSameName_HaveDifferentIds()
        {
            var cat1 = new Category("Homework", Avalonia.Media.Colors.LightBlue);
            var cat2 = new Category("Homework", Avalonia.Media.Colors.LightBlue);
            Assert.That(cat1.Id, Is.Not.EqualTo(cat2.Id));
        }
    }

    [TestFixture]
    public class EventConstructorTests
    {
        [Test]
        public void Constructor_TitleIsPreserved()
        {
            var @event = new Event(
                Title: "My Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid()
            );
            Assert.That(@event.Title, Is.EqualTo("My Task"));
        }

        [Test]
        public void Constructor_DescriptionIsPreserved()
        {
            var @event = new Event(
                Title: "Test",
                Description: "My Description",
                DueDate: DateTime.Today,
                CategoryId: new Guid()
            );
            Assert.That(@event.Description, Is.EqualTo("My Description"));
        }
 
        [Test]
        public void Constructor_ParentIdIsPreserved()
        {
            var parentId = Guid.NewGuid();
            var @event = new Event(
                Title: "Test",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                ParentId: parentId
            );
            Assert.That(@event.ParentId, Is.EqualTo(parentId));
        }
 
        [Test]
        public void Constructor_IsCompleteTrue_WhenPassedTrue()
        {
            var @event = new Event(
                Title: "Test",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                IsComplete: true
            );
            Assert.That(@event.IsComplete, Is.True);
        }
 
        [Test]
        public void Constructor_IsRepeating_WhenRepeatIntervalSet()
        {
            var @event = new Event(
                Title: "Test",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                RepeatInterval: RepeatInterval.Weekly
            );
            Assert.That(@event.IsRepeating, Is.True);
        }
 
        [Test]
        public void Constructor_TwoEvents_HaveDifferentIds()
        {
            var ev1 = new Event("A", "Test", DateTime.Today, new Guid());
            var ev2 = new Event("B", "Test", DateTime.Today, new Guid());
            Assert.That(ev1.Id, Is.Not.EqualTo(ev2.Id));
        }
    }

    [TestFixture]
    public class EventDueOnTests
    {
        [Test]
        public void DueOn_DateOnly_ReturnsTrueWhenMatches()
        {
            var date = new DateTime(2026, 6, 15);
            var @event = new Event("Test", "Test", date, new Guid());
            Assert.That(@event.DueOn(DateOnly.FromDateTime(date)), Is.True);
        }
 
        [Test]
        public void DueOn_DateOnly_ReturnsFalseWhenDifferent()
        {
            var date = new DateTime(2026, 6, 15);
            var @event = new Event("Test", "Test", date, new Guid());
            Assert.That(@event.DueOn(DateOnly.FromDateTime(date.AddDays(1))), Is.False);
        }
 
        [Test]
        public void DueOn_DateTime_ReturnsTrueWhenMatches()
        {
            var date = new DateTime(2026, 6, 15);
            var @event = new Event("Test", "Test", date, new Guid());
            Assert.That(@event.DueOn(date), Is.True);
        }
 
        [Test]
        public void NextOccurrenceFrom_WeeklyRepeat_ReturnsCorrectDate()
        {
            var dueDate = new DateTime(2025, 1, 1); // Wednesday
            var checkedDate = new DateTime(2025, 1, 3); // Friday
 
            var @event = new Event(
                Title: "Test",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                RepeatInterval: RepeatInterval.Weekly
            );
 
            Assert.That(@event.NextOccurrenceFrom(checkedDate), Is.EqualTo(new DateTime(2025, 1, 8)));
        }
    }

    [TestFixture]
    public class EventViewModelTests
    {
        [Test]
        public void EventViewModel_ColorFromMatchingCategory()
        {
            var cat = new Category("Work", Avalonia.Media.Colors.LightPink);
            var ev = new Event("Test", "Test", DateTime.Today, cat.Id);
            var evm = new EventViewModel(ev, new[] { cat });
            Assert.That(evm.Color, Is.EqualTo(Avalonia.Media.Colors.LightPink));
        }
 
        [Test]
        public void EventViewModel_ColorFallsBackToLightGray_WhenCategoryNotFound()
        {
            var ev = new Event("Test", "Test", DateTime.Today, Guid.NewGuid());
            var evm = new EventViewModel(ev, Array.Empty<Category>());
            Assert.That(evm.Color, Is.EqualTo(Avalonia.Media.Colors.LightGray));
        }
 
        [Test]
        public void EventViewModel_TitleMatchesEvent()
        {
            var cat = new Category("Work", Avalonia.Media.Colors.LightBlue);
            var ev = new Event("My Title", "Test", DateTime.Today, cat.Id);
            var evm = new EventViewModel(ev, new[] { cat });
            Assert.That(evm.Title, Is.EqualTo("My Title"));
        }
 
        [Test]
        public void EventViewModel_DescriptionMatchesEvent()
        {
            var cat = new Category("Work", Avalonia.Media.Colors.LightBlue);
            var ev = new Event("Test", "My Description", DateTime.Today, cat.Id);
            var evm = new EventViewModel(ev, new[] { cat });
            Assert.That(evm.Description, Is.EqualTo("My Description"));
        }
 
        [Test]
        public void EventViewModel_DueDateMatchesEvent()
        {
            var date = new DateTime(2026, 5, 10);
            var cat = new Category("Work", Avalonia.Media.Colors.LightBlue);
            var ev = new Event("Test", "Test", date, cat.Id);
            var evm = new EventViewModel(ev, new[] { cat });
            Assert.That(evm.DueDate, Is.EqualTo(date));
        }
    }

    [TestFixture]
    public class MainWindowViewModelTests
    {
        [Test]
        public void Constructor_CategoriesInitialized()
        {
            var vm = new MainWindowViewModel();
            Assert.That(vm.Categories.Count, Is.GreaterThan(0));
        }
 
        [Test]
        public void Constructor_TasksEmpty()
        {
            var vm = new MainWindowViewModel();
            Assert.That(vm.Tasks.Count, Is.EqualTo(0));
        }
 
        [Test]
        public void SaveTask_AddsToTasksAndTaskViews()
        {
            var vm = new MainWindowViewModel();
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
            var vm = new MainWindowViewModel();
            vm.NewTask.Title = "";
            vm.NewTask.Category = vm.Categories[0];
            vm.SaveTaskCommand.Execute(null);
            Assert.That(vm.Tasks.Count, Is.EqualTo(0));
        }
 
        [Test]
        public void SaveTask_DoesNothing_WhenCategoryNull()
        {
            var vm = new MainWindowViewModel();
            vm.NewTask.Title = "Test";
            vm.NewTask.Category = null;
            vm.SaveTaskCommand.Execute(null);
            Assert.That(vm.Tasks.Count, Is.EqualTo(0));
        }
 
        [Test]
        public void SaveTask_ResetsNewTask_AfterSaving()
        {
            var vm = new MainWindowViewModel();
            vm.NewTask.Title = "New Task";
            vm.NewTask.Category = vm.Categories[0];
            vm.SaveTaskCommand.Execute(null);
            Assert.That(vm.NewTask.Title, Is.EqualTo(string.Empty));
        }
 
        [Test]
        public void SaveTask_TaskViewColorMatchesCategory()
        {
            var vm = new MainWindowViewModel();
            vm.NewTask.Title = "Test";
            vm.NewTask.Category = vm.Categories[0];
            vm.SaveTaskCommand.Execute(null);
            Assert.That(vm.TaskViews[0].Color, Is.EqualTo(vm.Categories[0].Color));
        }
    } 
}
