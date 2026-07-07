using System.Windows;
using BanooPaz.WPF.ViewModels;

namespace BanooPaz.WPF;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
