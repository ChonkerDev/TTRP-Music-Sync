using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Music_Synchronizer.Data;
using Music_Synchronizer.Services;
using Music_Synchronizer.Services.Storage;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

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
    }

    public bool IsHosting => _musicHost.IsRunning;

    [ObservableProperty] private string _portInput;
    [ObservableProperty] private bool _isPlayingAudio;
    [ObservableProperty] private bool _downloadOverlayActive;
    [ObservableProperty] private string _downloadVideoAudioText;
    [ObservableProperty] private double _downloadVideoProgress;


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
    }

    [RelayCommand]
    public void StopHost() {
        if (!_musicHost.IsRunning) return;
        _musicHost.Stop();
        NotificationService.Notify("Host Stopped", "", NotificationType.Success);
        OnPropertyChanged(nameof(IsHosting));
    }

    [RelayCommand]
    public void TogglePLay() {
        if (!_naudioPlayerService.IsMediaLoaded()) return;

        if (_naudioPlayerService.IsPlaying) {
            _naudioPlayerService.Pause();
            IsPlayingAudio = false;
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
                AudioFileData fileData = new AudioFileData() {
                    FilePath = result.Data,
                    FileName = Path.GetFileName(result.Data),
                    FileId = Path.GetFileNameWithoutExtension(result.Data),
                };
                    
                _audioFilesRepositoryStorageService.DataInstance.AudioFileDataDictionary[fileData.FileName] = fileData;
            }
            
        }
        catch (Exception e) {
            NotificationService.Notify("Download Failed", e.Message, NotificationType.Error);
        }
        finally {
            DownloadOverlayActive = false;
        }
    }

    public void OnVolumeSliderChanged(double newVolume) {
        _naudioPlayerService.Volume = (float)newVolume / 100f;
    }
}