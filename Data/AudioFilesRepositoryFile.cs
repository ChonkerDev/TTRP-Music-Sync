using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Music_Synchronizer.Data;

public class AudioFilesRepositoryFile {

    [JsonInclude] private List<AudioFileData> AudioFileData { get; set; } = new();

    [JsonIgnore] public Dictionary<string, AudioFileData> AudioFileDataDictionary { get; set; } = new();

    public void PreSave() {
        AudioFileData = AudioFileDataDictionary.Values.ToList();
    }

    public void PostLoad() {
        AudioFileDataDictionary = new();
        foreach (var audioFileData in AudioFileData) {
            AudioFileDataDictionary.Add(audioFileData.FileId, audioFileData);
        }
    }
}