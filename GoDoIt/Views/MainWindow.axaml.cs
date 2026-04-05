using Avalonia.Controls;
using Avalonia.Input;
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
}
