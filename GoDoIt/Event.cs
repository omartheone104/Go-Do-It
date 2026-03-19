using System;
namespace GoDoIt;

public record Event(int Id, string Title, string Description, DateTime DueDate, int CategoryId, int? ParentId, bool IsComplete, TimeSpan? RepeatInterval)
{
    /// <summary>
    /// Checks if the event is due today, when the event is not complete and the DueDate is the same as the current date
    /// </summary>
    /// <returns>true if event is Due, false otherwise</returns>
    public bool DueToday() => !IsComplete && (DueDate == DateTime.Today);
}