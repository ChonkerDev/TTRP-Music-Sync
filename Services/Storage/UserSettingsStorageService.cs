using System;
using System.IO;
using System.Text.Json;
using HMV_Player.Services.Storage;
using Music_Synchronizer.Data;

namespace Music_Synchronizer.Services.Storage;

public class UserSettingsStorageService : BaseSettingsStorageService<UserSettingsFile> {
    protected override string baseFolderPath => MusicSynchronizerPaths.ConfigStoragePath;
    protected override string savePathFileName => "UserSettings";
    public UserSettingsStorageService() {
        Load();
    }
}