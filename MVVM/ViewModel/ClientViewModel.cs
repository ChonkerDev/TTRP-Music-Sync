using System;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Music_Synchronizer.Services;
using Music_Synchronizer.Services.Storage;

namespace Music_Synchronizer.MVVM.ViewModel;

public partial class ClientViewModel : ObservableObject {
    private readonly MusicClient _musicClient;
    private readonly UserSettingsStorageService _userSettingsStorageService;

    public ClientViewModel(MusicClient musicClient, UserSettingsStorageService userSettingsStorageService) {
        _musicClient = musicClient;
        _userSettingsStorageService = userSettingsStorageService;
        PortInput = _userSettingsStorageService.UserSettingsFile.DefaultPortToConnectTo.ToString();
        IpInput = _userSettingsStorageService.UserSettingsFile.DefaultIpToConnectTo;
    }

    [ObservableProperty] private string _portInput;
    [ObservableProperty] private string _ipInput;

    public bool IsConnected => _musicClient.IsConnected;
    
    private Task _connectToHostTask; 

    [RelayCommand]
    public async Task ConnectToHost() {
        try {
            if (!IPAddress.TryParse(IpInput, out var ip)) {
                NotificationService.Notify("Please enter a valid IP address", "", NotificationType.Error);
                return;
            }

            _userSettingsStorageService.UserSettingsFile.DefaultIpToConnectTo = IpInput;


            if (!int.TryParse(PortInput, out var port)) {
                NotificationService.Notify("Please enter a valid port number", "", NotificationType.Error);
                return;
            }

            _userSettingsStorageService.UserSettingsFile.DefaultPortToConnectTo = port;

            _userSettingsStorageService.Save();
            await _musicClient.ConnectToHost(_userSettingsStorageService.UserSettingsFile.DefaultIpToConnectTo,
                _userSettingsStorageService.UserSettingsFile.DefaultPortToConnectTo);
            OnPropertyChanged(nameof(IsConnected));
        }
        catch (Exception e) {
            NotificationService.Notify("Unable To Connect To Host", e.Message, NotificationType.Error);
        }
    }
}