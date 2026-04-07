using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using GoDoIt;
using GoDoIt.ViewModels;

namespace GoDoIt.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnTaskCardPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border { Tag: EventViewModel evm } &&
            DataContext is MainWindowViewModel vm)
        {
            vm.SelectTaskCommand.Execute(evm);
        }
    }

    private void OnDeleteCategoryClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button { Tag: Category cat } &&
            DataContext is MainWindowViewModel vm)
        {
            vm.DeleteCategoryCommand.Execute(cat);
        }
    }

    private void OnColorSwatchPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border { Tag: Color color } && 
            DataContext is MainWindowViewModel vm)
        { 
            vm.NewCategory.SelectedColor = color;
        } 
    }
}
