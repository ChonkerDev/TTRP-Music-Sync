using System;
using Music_Synchronizer.Data;

namespace Music_Synchronizer.MVVM.Model;

public class MusicTableEntryModel : IEquatable<MusicTableEntryModel>, IComparable<MusicTableEntryModel> {
    public string FileId { get; set; }
    public string FileName { get; set; }
    public bool IsVisible { get; set; }
    public string FileUrl { get; set; }

    public MusicTableEntryModel(AudioFileData audioFileData) {
        FileId = audioFileData.FileId;
        FileName = audioFileData.FileName;
        FileUrl = audioFileData.FileUrl;
    }
    

    public bool Equals(MusicTableEntryModel? other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return FileId == other.FileId;
    }

    public override bool Equals(object? obj) {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((MusicTableEntryModel)obj);
    }

    public override int GetHashCode() {
        return FileId.GetHashCode();
    }

    public int CompareTo(MusicTableEntryModel? other) {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return string.Compare(FileId, other.FileId, StringComparison.Ordinal);
    }
}