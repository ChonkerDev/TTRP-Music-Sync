using System;
using System.IO;

namespace Music_Synchronizer.Data;

public static class MusicSynchronizerPaths {
    public static readonly string RequiredToolsPath = Path.Combine(AppContext.BaseDirectory, "External Tools");
    public static readonly string FFmpegPath = Path.Combine(RequiredToolsPath, "ffmpeg.exe");
    public static readonly string ytdlpPath = Path.Combine(RequiredToolsPath, "yt-dlp.exe");
    
    public static readonly string MusicStoragePath = Path.Combine(AppContext.BaseDirectory, "Music Storage");
    public static readonly string ConfigStoragePath = Path.Combine(AppContext.BaseDirectory, "Config");
    public static readonly string UserSettingsFullPath = Path.Combine(ConfigStoragePath, "UserSettings.json");
}