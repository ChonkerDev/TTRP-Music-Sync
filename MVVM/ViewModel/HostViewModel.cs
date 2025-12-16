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

    public ObservableCollection<MusicTableEntryModel> MusicList { get; }

    private Dictionary<string, MusicTableEntryModel> _musicData = new();

    private MusicTableEntryModel? _currentMusicData;


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

        _musicHost.OnClientConnected += client => {
            _ = HandleClientConnected(client);
        };
    }

    private async Task HandleClientConnected(TcpClient client) {
        if (IsPlayingAudio) {
            var syncMessage =  new SyncMessage {
                Type = MessageType.LoadAndPlay,
                FileId = _currentMusicData.FileId,
                FileUrl = _currentMusicData.FileUrl,
            };
            await _musicHost.BroadcastMessage(syncMessage, client);
        }
    }

    [RelayCommand]
    public void StopHost() {
        if (!_musicHost.IsRunning) return;
        _musicHost.Stop();
        _naudioPlayerService.Stop();
        NotificationService.Notify("Host Stopped", "", NotificationType.Success);
        OnPropertyChanged(nameof(IsHosting));
        
    }

    [RelayCommand]
    public async Task TogglePLay() {
        if (!_naudioPlayerService.IsMediaLoaded()) return;

        if (_naudioPlayerService.IsPlaying) {
            _naudioPlayerService.Pause();
            var syncMessage = new SyncMessage();
            syncMessage.Type = MessageType.Play;
            IsPlayingAudio = false;
            try {
                await _musicHost.BroadcastMessage(syncMessage);
            }
            catch (Exception e) {
                NotificationService.Notify("Error Broadcasting Play Message", e.Message, NotificationType.Error);
            }
        }
        else {
            _naudioPlayerService.Play();
            IsPlayingAudio = true;
            try {
                var syncMessage = new SyncMessage();
                syncMessage.Type = MessageType.Pause;
                await _musicHost.BroadcastMessage(syncMessage);
            }
            catch (Exception e) {
                NotificationService.Notify("Error Broadcasting Pause Message", e.Message, NotificationType.Error);
            }
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
        _naudioPlayerService.Play();
        _naudioPlayerService.Volume = .75f;
        SyncMessage syncMessage = new SyncMessage();
        syncMessage.Type = MessageType.LoadAndPlay;
        syncMessage.FileUrl = audioFileData.FileUrl;
        syncMessage.FileId = audioFileData.FileId;
        IsPlayingAudio = true;
        await _musicHost.BroadcastMessage(syncMessage);
    }

    public void OnVolumeSliderChanged(double newVolume) {
        _naudioPlayerService.Volume = (float)newVolume / 100f;
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