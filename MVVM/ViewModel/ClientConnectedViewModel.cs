using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Music_Synchronizer.Data;
using Music_Synchronizer.Services;
using Music_Synchronizer.Services.Storage;

namespace Music_Synchronizer.MVVM.ViewModel;

public partial class ClientConnectedViewModel : ObservableObject {
    private readonly MusicClient _musicClient;
    private readonly UserSettingsStorageService _userSettingsStorageService;
    private readonly YoutubeDownloadService _youtubeDownloadService;
    private readonly AudioFilesRepositoryStorageService _audioFilesRepositoryStorageService;
    private readonly NAudioPlayerService _nAudioPlayerService;


    public ClientConnectedViewModel(MusicClient musicClient, UserSettingsStorageService userSettingsStorageService,
        YoutubeDownloadService youtubeDownloadService, NAudioPlayerService nAudioPlayerService,
        AudioFilesRepositoryStorageService audioFilesRepositoryStorageService) {
        _musicClient = musicClient;
        _userSettingsStorageService = userSettingsStorageService;
        _youtubeDownloadService = youtubeDownloadService;
        _audioFilesRepositoryStorageService = audioFilesRepositoryStorageService;
        _nAudioPlayerService = nAudioPlayerService;
        _musicClient.OnMessageReceived += MusicClientOnOnMessageReceived;
        _musicClient.OnDisconnected += OnDisconnected;

        VolumeSlider = 75;
        IsPlaying = true; // initializing to true so music auto plays first time recieving request
    }

    private void MusicClientOnOnMessageReceived(PlayerState message) {
            if (message.FileLoaded) {
                _ = HandleLoad(message);
            }
            else {
                FileLoaded = false;
                _nAudioPlayerService.Stop();
            }

        
    }

    private void OnDisconnected() {
        NotificationService.Notify("Disconnected from host", "", NotificationType.Warning);
    }

    [ObservableProperty] private bool _isDownloadingFile;
    [ObservableProperty] private double _downloadProgress;
    [ObservableProperty] private string _downloadFileName;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private string _playingFileText;
    [ObservableProperty] private bool _fileLoaded;
    [ObservableProperty] private double _volumeSlider;


    [RelayCommand]
    public void TogglePlay() {
        if (IsPlaying) {
            _nAudioPlayerService.Pause();
            IsPlaying = false;
        }
        else {
            _nAudioPlayerService.Play();
            IsPlaying = true;
        }
    }

    private async Task HandleLoad(PlayerState message) {
        if (!_audioFilesRepositoryStorageService.DataInstance.AudioFileDataDictionary.TryGetValue(
                message.FileId, out var audioFile)) {
            DownloadFileName = message.FileId;
            bool success = await DownloadFile(message.FileUrl);
            DownloadFileName = string.Empty;
            if (!success) {
                return;
            }
        }

        string path = Path.Combine(MusicSynchronizerPaths.MusicStoragePath,
            _audioFilesRepositoryStorageService.DataInstance.AudioFileDataDictionary[message.FileId].FileName);
        _nAudioPlayerService.Load(path);
        FileLoaded = true;
        if (IsPlaying) {
            _nAudioPlayerService.Volume = (float)VolumeSlider / 100;
            _nAudioPlayerService.Play();
        }

        PlayingFileText = message.FileId;
    }

    private async Task<bool> DownloadFile(string link) {
        IsDownloadingFile = true;
        bool success = false;
        try {
            var result = await _youtubeDownloadService.Download(link, (progress => {
                DownloadProgress = progress.Progress;
                Console.WriteLine(progress.Progress);
            }));
            success = result.Success;
            if (success) {
                AudioFileData fileData = new AudioFileData(result, link);

                _audioFilesRepositoryStorageService.DataInstance.AudioFileDataDictionary[fileData.FileId] = fileData;
                _audioFilesRepositoryStorageService.Save();
            }
        }
        catch (Exception e) {
            NotificationService.Notify("Download Failed", e.Message, NotificationType.Error);
        }
        finally {
            IsDownloadingFile = false;
        }

        return success;
    }

    partial void OnVolumeSliderChanged(double value) {
        if (_nAudioPlayerService.IsPlaying) {
            _nAudioPlayerService.Volume = (float)value / 100;
        }
    }
}