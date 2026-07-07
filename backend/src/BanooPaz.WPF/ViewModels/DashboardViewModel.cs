using System.Net.Http;
using BanooPaz.WPF.Services.Api;
using BanooPaz.Contracts.Admin;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BanooPaz.WPF.ViewModels;

public sealed class DashboardViewModel : ObservableObject
{
    private readonly IAdminDashboardApiClient _apiClient;
    private AdminDashboardSummaryDto? _summary;
    private DateTime? _lastLoadedAt;
    private bool _isBusy;
    private string? _errorMessage;

    public DashboardViewModel(IAdminDashboardApiClient apiClient)
    {
        _apiClient = apiClient;
        LoadCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        RefreshCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
    }

    public AdminDashboardSummaryDto? Summary
    {
        get => _summary;
        private set
        {
            if (!SetProperty(ref _summary, value))
            {
                return;
            }

            OnPropertyChanged(nameof(MenuStatusText));
            OnPropertyChanged(nameof(LastLoadedText));
        }
    }

    public DateTime? LastLoadedAt
    {
        get => _lastLoadedAt;
        private set
        {
            if (SetProperty(ref _lastLoadedAt, value))
            {
                OnPropertyChanged(nameof(LastLoadedText));
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                LoadCommand.NotifyCanExecuteChanged();
                RefreshCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string MenuStatusText => Summary?.IsTodayMenuOpen == true
        ? "باز"
        : "بسته / تعریف‌نشده";

    public string LastLoadedText => LastLoadedAt.HasValue
        ? $"آخرین به‌روزرسانی: {LastLoadedAt.Value:t}"
        : "هنوز به‌روزرسانی نشده";

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }

    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            Summary = await _apiClient.GetTodayAsync();
            LastLoadedAt = DateTime.Now;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"دریافت خلاصه داشبورد ناموفق بود: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
