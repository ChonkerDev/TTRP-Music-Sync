using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Music_Synchronizer.Data;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace Music_Synchronizer.Services;

public class YoutubeDownloadService {
    private readonly YoutubeDL _youtubeDL;

    public YoutubeDownloadService() {
        Directory.CreateDirectory(MusicSynchronizerPaths.RequiredToolsPath);
        Directory.CreateDirectory(MusicSynchronizerPaths.MusicStoragePath);
        _youtubeDL = new();
        _youtubeDL.FFmpegPath = MusicSynchronizerPaths.FFmpegPath;
        _youtubeDL.YoutubeDLPath = MusicSynchronizerPaths.ytdlpPath;

        _youtubeDL.OutputFolder = MusicSynchronizerPaths.MusicStoragePath;
    }

    public async Task<RunResult<string>> Download(string link, Action<DownloadProgress> OnProgress) {
        var progress = new Progress<DownloadProgress>(p => {
            OnProgress?.Invoke(p);
        });
        RunResult<string> result = await _youtubeDL.RunAudioDownload(link, AudioConversionFormat.Mp3, CancellationToken.None, progress);
        return result;
    }
}