using HMV_Player.Services.Storage;
using Music_Synchronizer.Data;

namespace Music_Synchronizer.Services.Storage;

public class AudioFilesRepositoryStorageService : BaseSettingsStorageService<AudioFilesRepositoryFile> {
    protected override string baseFolderPath => MusicSynchronizerPaths.ConfigStoragePath;
    protected override string savePathFileName => "Music Files Repository";

    public override void LoadObjectPostProcessing(AudioFilesRepositoryFile objectLoaded) {
        objectLoaded.PostLoad();
    }

    protected override void SaveObjectPreProcessing(AudioFilesRepositoryFile objectToSave) {
        objectToSave.PreSave();
    }
}