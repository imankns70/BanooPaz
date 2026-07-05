using System.Windows;
using System.Windows.Controls;
using BanooPaz.Admin.Wpf.ViewModels;

namespace BanooPaz.Admin.Wpf.Views;

public partial class LoginView : UserControl
{
    private bool _isSyncingPassword;

    public LoginView() => InitializeComponent();

    private LoginViewModel? ViewModel => DataContext as LoginViewModel;

    private void PasswordInput_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_isSyncingPassword || ViewModel is null)
        {
            return;
        }

        ViewModel.Password = PasswordInput.Password;
    }

    private void PasswordVisibilityToggle_OnChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        _isSyncingPassword = true;
        try
        {
            PasswordInput.Password = ViewModel.Password;
            VisiblePasswordInput.Text = ViewModel.Password;
        }
        finally
        {
            _isSyncingPassword = false;
        }

        if (ViewModel.IsPasswordVisible)
        {
            VisiblePasswordInput.Focus();
            VisiblePasswordInput.CaretIndex = VisiblePasswordInput.Text.Length;
        }
        else
        {
            PasswordInput.Focus();
        }
    }
}
