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

    public ClientViewModel(MusicClient musicClient, UserSettingsStorageService userSettingsStorageService,
        ClientConnectedViewModel clientConnectedViewModel) {
        _musicClient = musicClient;
        _userSettingsStorageService = userSettingsStorageService;

        PortInput = _userSettingsStorageService.DataInstance.DefaultPortToConnectTo.ToString();
        IpInput = _userSettingsStorageService.DataInstance.DefaultIpToConnectTo;
        ClientConnectedViewModel = clientConnectedViewModel;
    }

    [ObservableProperty] private string _portInput;
    [ObservableProperty] private string _ipInput;

    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private bool _connecting;


    public object ClientConnectedViewModel { get; }

    private Task _connectToHostTask;

    [RelayCommand]
    public async Task ConnectToHost() {
        if (!IPAddress.TryParse(IpInput, out var ip)) {
            NotificationService.Notify("Please enter a valid IP address", "", NotificationType.Error);
            return;
        }

        _userSettingsStorageService.DataInstance.DefaultIpToConnectTo = IpInput;


        if (!int.TryParse(PortInput, out var port)) {
            NotificationService.Notify("Please enter a valid port number", "", NotificationType.Error);
            return;
        }

        _userSettingsStorageService.DataInstance.DefaultPortToConnectTo = port;

        _userSettingsStorageService.Save();
        try {
            Connecting = true;
            _musicClient.OnConnected += OnConnected;
            await _musicClient.ConnectToHost(_userSettingsStorageService.DataInstance.DefaultIpToConnectTo,
                _userSettingsStorageService.DataInstance.DefaultPortToConnectTo);
            _musicClient.OnDisconnected += OnDisconnected;
        }
        catch (Exception e) {
            _musicClient.OnConnected -= OnConnected;
            NotificationService.Notify("Unable To Connect To Host", e.Message, NotificationType.Error);
            IsConnected = false;
        }
        finally {
            Connecting = false;
        }
    }

    private void OnDisconnected() {
        _musicClient.OnConnected -= OnConnected;
        _musicClient.OnDisconnected -= OnDisconnected;
        IsConnected = false;
        NotificationService.Notify("Disconnected from Host", "", NotificationType.Warning);
    }

    private void OnConnected() {
        IsConnected = true;
        NotificationService.Notify("Connected to Host", "", NotificationType.Success);
    }
}