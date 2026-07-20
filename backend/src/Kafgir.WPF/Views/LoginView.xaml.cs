using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Kafgir.WPF.ViewModels;

namespace Kafgir.WPF.Views;

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

    private void LoginInput_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || ViewModel?.LoginCommand is null)
        {
            return;
        }

        if (ViewModel.LoginCommand.CanExecute(null))
        {
            ViewModel.LoginCommand.Execute(null);
            e.Handled = true;
        }
    }
}
