using System;
using System.IO;
using System.Text.Json;

namespace HMV_Player.Services.Storage;

public abstract class BaseSettingsStorageService<T> where T : class, new() {
    protected abstract string baseFolderPath { get; } 
    protected abstract string savePathFileName { get; }

    public virtual void LoadObjectPostProcessing(T objectLoaded) {
        
    }

    protected virtual void SaveObjectPreProcessing(T objectToSave) {
        
    }
    
    public T DataInstance;

    public BaseSettingsStorageService() {
        DataInstance = Load();
    }

    protected string BuildFullFolderPath() {
        return Path.Combine(baseFolderPath, savePathFileName + ".json");
    }

    protected T Load() {
        string fullFolderPath = BuildFullFolderPath();
        if (!File.Exists(fullFolderPath)) {
            var instance = new T();
            Save(instance);
            return instance;
        }

        try {
            var json = File.ReadAllText(fullFolderPath);
            var instance = JsonSerializer.Deserialize<T>(json);

            if (instance == null) {
                instance = new T();
                Save(instance);
            }
            LoadObjectPostProcessing(instance);
            return instance;
        }
        catch(Exception e) {
            var instance = new T();
            Save(instance);
            return instance;
        }
    }
    
    private void Save(T objectToSave) {
        SaveObjectPreProcessing(objectToSave);
        string fullFolderPath = BuildFullFolderPath();
        Directory.CreateDirectory(Path.GetDirectoryName(fullFolderPath)!);
        File.WriteAllText(fullFolderPath, JsonSerializer.Serialize(objectToSave, new JsonSerializerOptions {
            WriteIndented = true
        }));
    }

    public void Save() {
        Save(DataInstance);
    }
}