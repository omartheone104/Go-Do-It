using System;
using Avalonia.Media;

namespace GoDoIt;

public class Category(string Name, Color Color)
{
    private readonly Guid id = Guid.NewGuid(); 
    public Guid Id => id;
    private string name = Name;
    public string Name => name;
    private Color color = Color;
    public Color Color => color;
}
