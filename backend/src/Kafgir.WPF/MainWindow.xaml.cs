using System.Windows;
using Kafgir.WPF.ViewModels;

namespace Kafgir.WPF;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
