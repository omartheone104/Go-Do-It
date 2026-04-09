using System;
using System.Collections.Generic;
using System.Linq;

namespace GoDoIt.ViewModels;
 
public class EventViewModel
{
    public Event Event { get; }
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
}
