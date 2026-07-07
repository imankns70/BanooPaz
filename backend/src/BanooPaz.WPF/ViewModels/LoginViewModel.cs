using System.Net;
using System.Net.Http;
using BanooPaz.WPF.Services.Api;
using BanooPaz.Contracts.Auth;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BanooPaz.WPF.ViewModels;

public sealed class LoginViewModel : ObservableObject
{
    private readonly IAuthApiClient _authApiClient;
    private readonly IApiHealthClient _apiHealthClient;
    private readonly IAdminSession _adminSession;
    private string _username = "admin";
    private string _password = string.Empty;
    private bool _isPasswordVisible;
    private bool _isBusy;
    private bool _isCheckingConnection;
    private bool? _isApiAvailable;
    private string? _errorMessage;
    private string? _connectionMessage;

    public LoginViewModel(
        IAuthApiClient authApiClient,
        IApiHealthClient apiHealthClient,
        IAdminSession adminSession)
    {
        _authApiClient = authApiClient;
        _apiHealthClient = apiHealthClient;
        _adminSession = adminSession;
        LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
        CheckConnectionCommand = new AsyncRelayCommand(CheckConnectionAsync, () => !IsCheckingConnection);
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

    public bool IsCheckingConnection
    {
        get => _isCheckingConnection;
        private set
        {
            if (SetProperty(ref _isCheckingConnection, value))
            {
                LoginCommand.NotifyCanExecuteChanged();
                CheckConnectionCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool? IsApiAvailable
    {
        get => _isApiAvailable;
        private set
        {
            if (SetProperty(ref _isApiAvailable, value))
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

    public string? ConnectionMessage
    {
        get => _connectionMessage;
        private set => SetProperty(ref _connectionMessage, value);
    }

    public IAsyncRelayCommand LoginCommand { get; }
    public IAsyncRelayCommand CheckConnectionCommand { get; }

    public void StartConnectionCheck()
    {
        _ = RunConnectionCheckSafelyAsync();
    }

    private bool CanLogin()
    {
        return !IsBusy
            && !string.IsNullOrWhiteSpace(Username)
            && !string.IsNullOrEmpty(Password);
    }

    private async Task CheckConnectionAsync()
    {
        IsCheckingConnection = true;
        ErrorMessage = null;
        ConnectionMessage = "در حال بررسی اتصال به سرور...";

        try
        {
            IsApiAvailable = await _apiHealthClient.IsApiAvailableAsync();
            ConnectionMessage = IsApiAvailable == true
                ? null
                : "ارتباط با سرور برقرار نیست. لطفاً اتصال اینترنت/شبکه و اجرای API را بررسی کنید.";
        }
        finally
        {
            IsCheckingConnection = false;
        }
    }

    private async Task RunConnectionCheckSafelyAsync()
    {
        try
        {
            await CheckConnectionAsync();
        }
        catch
        {
            IsApiAvailable = false;
            ConnectionMessage = "ارتباط با سرور برقرار نیست. لطفاً اتصال اینترنت/شبکه و اجرای API را بررسی کنید.";
            IsCheckingConnection = false;
        }
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

            _adminSession.Start(response);
            IsApiAvailable = true;
            ConnectionMessage = null;
            Password = string.Empty;
            LoginSucceeded?.Invoke(this, response);
        }
        catch (HttpRequestException exception)
        {
            if (exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                ErrorMessage = "نام کاربری یا رمز عبور درست نیست.";
            }
            else
            {
                IsApiAvailable = false;
                ConnectionMessage = "ارتباط با سرور برقرار نیست. لطفاً اتصال اینترنت/شبکه و اجرای API را بررسی کنید.";
            }
        }
        catch (TaskCanceledException)
        {
            IsApiAvailable = false;
            ConnectionMessage = "ارتباط با سرور بیش از حد طول کشید. لطفاً اتصال و اجرای API را بررسی کنید.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
