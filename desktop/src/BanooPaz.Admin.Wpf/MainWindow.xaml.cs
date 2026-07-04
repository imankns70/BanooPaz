using System.Windows;
using BanooPaz.Admin.Wpf.ViewModels;

namespace BanooPaz.Admin.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) =>
        {
            viewModel.Orders.LoadOrdersCommand.Execute(null);
            viewModel.Foods.LoadFoodsCommand.Execute(null);
            viewModel.DailyMenu.LoadMenuCommand.Execute(null);
        };
    }
}
