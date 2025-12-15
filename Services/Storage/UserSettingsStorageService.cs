using System;
using System.IO;
using System.Text.Json;
using Music_Synchronizer.Data;

namespace Music_Synchronizer.Services.Storage;

public class UserSettingsStorageService {
    public UserSettingsFile UserSettingsFile;
    public UserSettingsStorageService() {
        Load();
    }

    public void Load() {
        string path = Path.GetDirectoryName(MusicSynchronizerPaths.UserSettingsFullPath);
        Directory.CreateDirectory(path);
        
        string fullFolderPath = MusicSynchronizerPaths.UserSettingsFullPath;
        if (!File.Exists(fullFolderPath)) {
            UserSettingsFile = new UserSettingsFile();
            Save();
        }

        try {
            var json = File.ReadAllText(fullFolderPath);
            UserSettingsFile = JsonSerializer.Deserialize<UserSettingsFile>(json);

            if (UserSettingsFile == null) {
                UserSettingsFile = new UserSettingsFile();
                Save();
            }
        }
        catch(Exception e) {
            UserSettingsFile = new UserSettingsFile();
            Save();
        }
    }
    
    public void Save() {
        string fullFolderPath = MusicSynchronizerPaths.UserSettingsFullPath;
        Directory.CreateDirectory(Path.GetDirectoryName(fullFolderPath)!);
        File.WriteAllText(fullFolderPath, JsonSerializer.Serialize(UserSettingsFile, new JsonSerializerOptions {
            WriteIndented = true
        }));
    }
}