using System;
using System.Collections.Generic;
using System.Linq;

namespace GoDoIt.ViewModels;
 
public class EventViewModel
{
    public Event Event { get; private set; }
    public Avalonia.Media.Color Color { get; }
    public string Title => Event.Title;
    public string Description => Event.Description;
    public DateTime OccurrenceDate { get; }
    public DateTime DueDate => OccurrenceDate;
    public bool IsSubtask => Event.IsSubtask;
    public List<EventViewModel> Subtasks { get; } = new(); 
    public bool HasSubtasks => Subtasks.Count > 0;
    public bool IsComplete => Event.IsComplete; 

    public EventViewModel(Event ev, IEnumerable<Category> categories, DateTime? occurrenceDate = null)
    {
        Event = ev;
        OccurrenceDate = occurrenceDate ?? ev.DueDate; 
        Color = categories.FirstOrDefault(c => c.Id == ev.CategoryId)?.Color
                ?? Avalonia.Media.Colors.LightGray;
    }

    public void Reschedule(DateTime newDueDate)
    {
        var replacement = new Event(
            Event.Title,
            Event.Description,
            newDueDate,
            Event.CategoryId,
            Event.ParentId,
            Event.IsComplete,
            Event.RepeatInterval
        );

        replacement.Id = Event.Id;
        Event = replacement;
    }
}
