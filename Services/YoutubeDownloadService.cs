using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Music_Synchronizer.Data;

namespace Music_Synchronizer.Services;

public class YoutubeDownloadService {
    private readonly string baseCommand = "./yt-dlp -x --audio-format mp3 --audio-quality 0 ";
    private readonly string ytdlpPath;
    private const string baseArgs =
        "-x --audio-format mp3 --audio-quality 0";
    private readonly string outputDir = Path.Combine(AppContext.BaseDirectory, "Audio Storage");

    public YoutubeDownloadService() {
        ytdlpPath =
            Path.Combine(MusicSynchronizerPaths.YtdlpPath,
                OperatingSystem.IsWindows() ? "yt-dlp.exe" : "yt-dlp");
        
        Directory.CreateDirectory(outputDir);
        Directory.CreateDirectory(MusicSynchronizerPaths.YtdlpPath);
    }

    public async Task<bool> DownloadAudio(string youtubeLink) {
        if (!File.Exists(ytdlpPath))
            throw new FileNotFoundException("yt-dlp binary not found", ytdlpPath);

        var args =
            $"{baseArgs} " +
            $"-o \"{Path.Combine(outputDir, "%(title)s.%(ext)s")}\" " +
            $"\"{youtubeLink}\"";

        var psi = new ProcessStartInfo
        {
            FileName = ytdlpPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = AppContext.BaseDirectory
        };

        using var process = new Process { StartInfo = psi };

        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            // log stderr here if you want
            return false;
        }

        return true;
    }
}