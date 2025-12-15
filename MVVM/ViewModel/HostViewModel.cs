using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Music_Synchronizer.Services;
using Music_Synchronizer.Services.Storage;

namespace Music_Synchronizer.MVVM.ViewModel;

public partial class HostViewModel : ObservableObject {
    private readonly MusicHost _musicHost;
    private readonly UserSettingsStorageService _userSettingsStorageService;

    public HostViewModel(MusicHost musicHost, UserSettingsStorageService userSettingsStorageService) {
        _musicHost = musicHost;
        _userSettingsStorageService = userSettingsStorageService;
        PortInput = _userSettingsStorageService.UserSettingsFile.DefaultPortToHostOn.ToString();
    }

    public bool IsHosting => _musicHost.IsRunning;

    [ObservableProperty] private string _portInput;


    [RelayCommand]
    public async Task StartHost() {
        if (_musicHost.IsRunning) return;
        if (string.IsNullOrEmpty(_portInput) || !int.TryParse(_portInput, out var port)) {
            NotificationService.Notify("Host Failed To Start", $"Please enter a valid port", NotificationType.Error);
            return;
        }

        _userSettingsStorageService.UserSettingsFile.DefaultPortToHostOn = port;
        _userSettingsStorageService.Save();
        try {
            await _musicHost.StartHost(_userSettingsStorageService.UserSettingsFile.DefaultPortToHostOn);
        }
        catch (Exception e) {
            NotificationService.Notify("Host Failed To Start", e.Message, NotificationType.Error);
            return;
        }

        NotificationService.Notify("Host Started", "", NotificationType.Success);
        OnPropertyChanged(nameof(IsHosting));
    }

    [RelayCommand]
    public void StopHost() {
        if (!_musicHost.IsRunning) return;
        _musicHost.Stop();
        NotificationService.Notify("Host Stopped", "", NotificationType.Success);
        OnPropertyChanged(nameof(IsHosting));
    }
}