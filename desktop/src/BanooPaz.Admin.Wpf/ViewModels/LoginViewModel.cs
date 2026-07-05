using System.Net.Http;
using BanooPaz.Admin.Wpf.Services.Api;
using BanooPaz.Contracts.Auth;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BanooPaz.Admin.Wpf.ViewModels;

public sealed class LoginViewModel : ObservableObject
{
    private readonly IAuthApiClient _authApiClient;
    private string _username = "admin";
    private string _password = string.Empty;
    private bool _isPasswordVisible;
    private bool _isBusy;
    private string? _errorMessage;

    public LoginViewModel(IAuthApiClient authApiClient)
    {
        _authApiClient = authApiClient;
        LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
    }

    public event EventHandler<AdminLoginResponse>? LoginSucceeded;

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                LoginCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                LoginCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsPasswordVisible
    {
        get => _isPasswordVisible;
        set => SetProperty(ref _isPasswordVisible, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                LoginCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public IAsyncRelayCommand LoginCommand { get; }

    private bool CanLogin()
    {
        return !IsBusy
            && !string.IsNullOrWhiteSpace(Username)
            && !string.IsNullOrEmpty(Password);
    }

    private async Task LoginAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var response = await _authApiClient.LoginAsync(new AdminLoginRequest
            {
                Username = Username.Trim(),
                Password = Password
            });

            Password = string.Empty;
            LoginSucceeded?.Invoke(this, response);
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "نام کاربری یا رمز عبور درست نیست.";
        }
        catch (TaskCanceledException)
        {
            ErrorMessage = "ارتباط با سرور بیش از حد طول کشید.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
