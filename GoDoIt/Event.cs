using System;
namespace GoDoIt;

public record Event(int Id, string Title, string Description, DateTime DueDate, int CategoryId, int? ParentId, bool IsComplete, TimeSpan? RepeatInterval)
{
    public bool DueToday() => throw new NotImplementedException();
}