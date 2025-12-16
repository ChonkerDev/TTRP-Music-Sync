using System.IO;
using YoutubeDLSharp;

namespace Music_Synchronizer.Data;

public class AudioFileData {
    public string FileName { get; set; }
    public string FileId { get; set; }
    
    public string FileUrl { get; set; }

    public AudioFileData() { } // so deserialization has a constructor
    
    public AudioFileData(RunResult<string> result, string videoUrl) {
        FileName = Path.GetFileName(result.Data);
        FileId = Path.GetFileNameWithoutExtension(result.Data);
        FileUrl = videoUrl;
    }
}