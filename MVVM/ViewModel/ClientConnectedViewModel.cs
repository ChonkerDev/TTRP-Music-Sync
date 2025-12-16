using System.IO;
using System.Threading.Tasks;
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
            IsDownloadingFile = true;
            await _youtubeDownloadService.Download(message.FileUrl, progress => DownloadProgress = progress.Progress);
            IsDownloadingFile = false;
        }
        
        _nAudioPlayerService.Load(Path.Combine(MusicSynchronizerPaths.MusicStoragePath, audioFile.FileName));
        _nAudioPlayerService.Play();
        PlayingFileText = audioFile.FileName;
        IsPlaying = true;
    }
}