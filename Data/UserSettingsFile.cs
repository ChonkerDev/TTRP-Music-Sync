namespace Music_Synchronizer.Data;

public class UserSettingsFile {
    public int DefaultPortToConnectTo { get; set; } = 4200;
    public string DefaultIpToConnectTo { get; set; } = "127.0.0.1";
    
    public int DefaultPortToHostOn { get; set; } = 4200;
}