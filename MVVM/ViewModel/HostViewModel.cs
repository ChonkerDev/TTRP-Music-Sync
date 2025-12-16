using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Music_Synchronizer.Data;
using Music_Synchronizer.MVVM.Model;
using Music_Synchronizer.Services;
using Music_Synchronizer.Services.Storage;

namespace Music_Synchronizer.MVVM.ViewModel;

public partial class HostViewModel : ObservableObject {
    private readonly MusicHost _musicHost;
    private readonly UserSettingsStorageService _userSettingsStorageService;
    private readonly AudioFilesRepositoryStorageService _audioFilesRepositoryStorageService;
    private readonly NAudioPlayerService _naudioPlayerService;
    private readonly YoutubeDownloadService _youtubeDownloadService;

    public HostViewModel(MusicHost musicHost, UserSettingsStorageService userSettingsStorageService,
        NAudioPlayerService naudioPlayerService, YoutubeDownloadService youtubeDownloadService,
        AudioFilesRepositoryStorageService audioFilesRepositoryStorageService) {
        _musicHost = musicHost;
        _userSettingsStorageService = userSettingsStorageService;
        _naudioPlayerService = naudioPlayerService;
        _youtubeDownloadService = youtubeDownloadService;
        _audioFilesRepositoryStorageService = audioFilesRepositoryStorageService;
        PortInput = _userSettingsStorageService.DataInstance.DefaultPortToHostOn.ToString();

        MusicList = new();

        RefreshMusicList();
    }

    public bool IsHosting => _musicHost.IsRunning;

    [ObservableProperty] private string _portInput;
    [ObservableProperty] private bool _isPlayingAudio;
    [ObservableProperty] private bool _downloadOverlayActive;
    [ObservableProperty] private string _downloadVideoAudioText;
    [ObservableProperty] private double _downloadVideoProgress;
    [ObservableProperty] private string _searchBoxText;
    [ObservableProperty] private bool _isFileLoaded;
    [ObservableProperty] private string _fileLoadedText;

    private float _currentVolume = .75f;

    public ObservableCollection<MusicTableEntryModel> MusicList { get; }

    private ObservableCollection<string> ConnectedClients = new();

    public int ClientsConnected => ConnectedClients.Count; 
    
    private MusicTableEntryModel? _currentMusicData;
    
    private PlayerState _currentPlayerState = new();



    [RelayCommand]
    public async Task StartHost() {
        if (_musicHost.IsRunning) return;
        if (string.IsNullOrEmpty(_portInput) || !int.TryParse(_portInput, out var port)) {
            NotificationService.Notify("Host Failed To Start", $"Please enter a valid port", NotificationType.Error);
            return;
        }

        _userSettingsStorageService.DataInstance.DefaultPortToHostOn = port;
        _userSettingsStorageService.Save();
        try {
            await _musicHost.StartHost(_userSettingsStorageService.DataInstance.DefaultPortToHostOn);
        }
        catch (Exception e) {
            NotificationService.Notify("Host Failed To Start", e.Message, NotificationType.Error);
            return;
        }

        NotificationService.Notify("Host Started", "", NotificationType.Success);
        OnPropertyChanged(nameof(IsHosting));

        ConnectedClients = new();
        _musicHost.OnClientConnected += client => {
            _ = HandleClientConnected(client);
        };
        _musicHost.OnClientDisconnected += OnClientDisconnected;
    }

    private void OnClientDisconnected(TcpClient obj) {
        ConnectedClients.Add("Client");
        NotificationService.Notify("Client Disconnected", "", NotificationType.Information);
        OnPropertyChanged(nameof(ClientsConnected));
    }

    private async Task HandleClientConnected(TcpClient client) {
        ConnectedClients.Remove("Client");
        NotificationService.Notify("Client Connected", "", NotificationType.Information);
            await _musicHost.BroadcastMessage(_currentPlayerState, client);
            OnPropertyChanged(nameof(ClientsConnected));

    }

    [RelayCommand]
    public void StopHost() {
        if (!_musicHost.IsRunning) return;
        _musicHost.Stop();
        _naudioPlayerService.Stop();
        IsFileLoaded = false;
        FileLoadedText = string.Empty;
        NotificationService.Notify("Host Stopped", "", NotificationType.Success);
        OnPropertyChanged(nameof(IsHosting));
        
    }

    [RelayCommand]
    public void TogglePLay() {
        if (!_naudioPlayerService.IsMediaLoaded()) return;

        if (_naudioPlayerService.IsPlaying) {
            IsPlayingAudio = false;
            _naudioPlayerService.Pause();
        }
        else {
            _naudioPlayerService.Play();
            IsPlayingAudio = true;
        }
    }

    [RelayCommand]
    public async Task DownloadAudioFromVideo() {
        DownloadOverlayActive = true;
        string link = DownloadVideoAudioText;
        try {
            var result = await _youtubeDownloadService.Download(link, (progress => {
                DownloadVideoProgress = progress.Progress;
                Console.WriteLine(progress.Progress);
            }));
            if (result.Success) {
                AudioFileData fileData = new AudioFileData(result,link );

                _audioFilesRepositoryStorageService.DataInstance.AudioFileDataDictionary[fileData.FileId] = fileData;
                _audioFilesRepositoryStorageService.Save();
                
                RefreshMusicList();
            }
        }
        catch (Exception e) {
            NotificationService.Notify("Download Failed", e.Message, NotificationType.Error);
        }
        finally {
            DownloadOverlayActive = false;
        }
    }

    [RelayCommand]
    public void DeleteMusic(MusicTableEntryModel audioFileData) {
        _audioFilesRepositoryStorageService.DataInstance.AudioFileDataDictionary.Remove(audioFileData.FileId);
        try {
            File.Delete(Path.Combine(MusicSynchronizerPaths.MusicStoragePath, audioFileData.FileId));
        }
        catch (FileNotFoundException e) {
            NotificationService.Notify("File Delete Failed", e.Message, NotificationType.Error);
        }
        _audioFilesRepositoryStorageService.Save();
        RefreshMusicList();
    }

    [RelayCommand]
    public async Task PlayMusic(MusicTableEntryModel audioFileData) {
        _naudioPlayerService.Load(Path.Combine(MusicSynchronizerPaths.MusicStoragePath, audioFileData.FileName));
        IsFileLoaded = true;
        FileLoadedText = audioFileData.FileName;
        _naudioPlayerService.Play();
        _naudioPlayerService.Volume = _currentVolume;
        _currentPlayerState.FileLoaded = true;
        _currentPlayerState.FileUrl = audioFileData.FileUrl;
        _currentPlayerState.FileId = audioFileData.FileId;
        _currentMusicData = audioFileData;
        IsPlayingAudio = true;
        await _musicHost.BroadcastMessage(_currentPlayerState);
    }

    [RelayCommand]
    public async Task StopMusic() {
        _naudioPlayerService.Stop();
        _currentMusicData = null;
        _currentPlayerState.FileLoaded = false;
        _currentPlayerState.FileUrl = null;
        _currentPlayerState.FileId = null;
        await _musicHost.BroadcastMessage(_currentPlayerState);
    }

    public void OnVolumeSliderChanged(double newVolume) {
        _currentVolume = (float)newVolume / 100f;
        _naudioPlayerService.Volume = _currentVolume;
    }

    partial void OnSearchBoxTextChanged(string searchBoxText) {
        RefreshMusicList();
    }

    private void RefreshMusicList() {
        //TODO: this should be implemented using filters, just rebuilding list for now, this is inefficient
        MusicList.Clear();
        if (string.IsNullOrEmpty(SearchBoxText)) {
            foreach (var audioFileData in _audioFilesRepositoryStorageService.DataInstance.AudioFileDataDictionary.Values) {
                MusicList.Add(new (audioFileData));
            }

            return;
        }
        foreach (var musicTableEntryModel in _audioFilesRepositoryStorageService.DataInstance.AudioFileDataDictionary.Values.Where(data => data.FileId.Contains(SearchBoxText))
                     .Select(audioFileData => new MusicTableEntryModel(audioFileData))) {
            MusicList.Add(musicTableEntryModel);
        }
        
    }
}