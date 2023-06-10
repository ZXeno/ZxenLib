namespace ZxenLib.Configuration;

using System.Runtime.InteropServices;
using Enums;

public class OsPlatformInfo
{
    public OperatingPlatform Platform { get; set; }
    
    // Default to true
    public bool IsSupported { get; private set; } = true;

    // Converts a system OSPlatform object to something usable. 
    public static OsPlatformInfo FromRuntimeInfo()
    {
        OperatingPlatform platform = GetOperatingPlatform(RuntimeInformation.OSDescription);
        OsPlatformInfo osInfo = new()
        {
            Platform = platform,
            IsSupported = platform is OperatingPlatform.Linux or OperatingPlatform.Windows,
        };
        
        return osInfo;
    }

    private static OperatingPlatform GetOperatingPlatform(string systemPlatform)
    {
        // There has to be a better way to determine what OS we're running on...
        
        if (systemPlatform.Contains("Windows"))
        {
            return OperatingPlatform.Windows;
        }

        if (systemPlatform.Contains("Linux"))
        {
            return OperatingPlatform.Linux;
        }

        if (systemPlatform.Contains("Apple"))
        {
            return OperatingPlatform.Mac;
        }

        return OperatingPlatform.Unsupported;
    }
}