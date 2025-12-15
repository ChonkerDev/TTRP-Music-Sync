using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Music_Synchronizer.Services;

namespace Music_Synchronizer.MVVM.ViewModel;

public partial class MainWindowViewModel : ObservableObject {

    private readonly ClientViewModel _clientViewModel;
    private readonly HostViewModel _hostViewModel;
    
    public MainWindowViewModel(ClientViewModel clientViewModel, HostViewModel hostViewModel) {
        _hostViewModel =  hostViewModel;
        _clientViewModel = clientViewModel;
    }

    public bool ShowStartupOptions => CurrentView != null;

    
    [ObservableProperty]
    private ObservableObject? _currentView;
    
    [RelayCommand]
    public void GoToHostPage() {
        CurrentView = _hostViewModel;
        OnPropertyChanged(nameof(ShowStartupOptions));
    }

    [RelayCommand]
    public void GoToClientPage() {
        CurrentView = _clientViewModel;
        OnPropertyChanged(nameof(ShowStartupOptions));
    }
}