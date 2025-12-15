using System;
using System.IO;

namespace Music_Synchronizer.Data;

public static class MusicSynchronizerPaths {
    public static readonly string YtdlpPath = Path.Combine(AppContext.BaseDirectory, "yt-dlp");
    public static readonly string MusicStoragePath = Path.Combine(AppContext.BaseDirectory, "Music Storage");
    public static readonly string UserSettingsFullPath = Path.Combine(AppContext.BaseDirectory, "Config", "UserSettings.json");
}