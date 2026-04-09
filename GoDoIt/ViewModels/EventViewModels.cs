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
    public DateTime DueDate => Event.DueDate;

    public EventViewModel(Event ev, IEnumerable<Category> categories)
    {
        Event = ev;
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
