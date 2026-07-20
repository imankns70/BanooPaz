using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Kafgir.WPF.Models;

public sealed class PaginationViewModel<T> : ObservableObject
{
    private IReadOnlyList<T> _allItems = [];
    private int _currentPage = 1;

    public PaginationViewModel(int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        PageSize = pageSize;
        PreviousPageCommand = new RelayCommand(PreviousPage, () => CurrentPage > 1);
        NextPageCommand = new RelayCommand(NextPage, () => CurrentPage < TotalPages);
    }

    public ObservableCollection<T> Items { get; } = [];
    public int PageSize { get; }
    public int TotalItems => _allItems.Count;
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
    public string PageText => $"صفحه {CurrentPage:N0} از {TotalPages:N0}";
    public string TotalItemsText => $"{TotalItems:N0} مورد";

    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (SetProperty(ref _currentPage, value))
            {
                ApplyPage();
            }
        }
    }

    public IRelayCommand PreviousPageCommand { get; }
    public IRelayCommand NextPageCommand { get; }

    public void SetItems(IEnumerable<T>? items, bool resetPage = true)
    {
        _allItems = items?.ToList() ?? [];
        _currentPage = resetPage ? 1 : Math.Min(CurrentPage, TotalPages);
        OnPropertyChanged(nameof(CurrentPage));
        ApplyPage();
    }

    public void MoveToLastPage()
    {
        CurrentPage = TotalPages;
    }

    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
        }
    }

    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
        }
    }

    private void ApplyPage()
    {
        Items.Clear();
        foreach (var item in _allItems.Skip((CurrentPage - 1) * PageSize).Take(PageSize))
        {
            Items.Add(item);
        }

        OnPropertyChanged(nameof(TotalItems));
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(PageText));
        OnPropertyChanged(nameof(TotalItemsText));
        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();
    }
}
