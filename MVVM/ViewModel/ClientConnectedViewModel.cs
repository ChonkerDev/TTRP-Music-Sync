using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
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
        _musicClient.OnMessageReceived += message => {
            if (message.Type == MessageType.LoadAndPlay) {
                _ = HandleLoadAndPlay(message);
            } else if (message.Type == MessageType.Pause) {
                _nAudioPlayerService.Pause();
            } else if (message.Type == MessageType.Play) {
                _nAudioPlayerService.Play();
            }
        };
    }

    [ObservableProperty] private bool _isDownloadingFile;
    [ObservableProperty] private double _downloadProgress;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private string _playingFileText;



    private async Task HandleLoadAndPlay(SyncMessage message) {
        if (!_audioFilesRepositoryStorageService.DataInstance.AudioFileDataDictionary.TryGetValue(
                message.FileId, out var audioFile)) {
            await DownloadFile(message.FileUrl);
        }
        
        _nAudioPlayerService.Load(Path.Combine(MusicSynchronizerPaths.MusicStoragePath, audioFile.FileName));
        _nAudioPlayerService.Play();
        PlayingFileText = audioFile.FileName;
        IsPlaying = true;
    }

    private async Task DownloadFile(string link) {
        IsDownloadingFile = true;
        try {
            var result = await _youtubeDownloadService.Download(link, (progress => {
                DownloadProgress = progress.Progress;
                Console.WriteLine(progress.Progress);
            }));
            if (result.Success) {
                AudioFileData fileData = new AudioFileData(result,link);

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
    } 
}