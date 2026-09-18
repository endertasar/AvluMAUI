using Android.Util;

namespace AvluMAUI.Helpers;

public static partial class StartupLogger
{
    static partial void PlatformWrite(string line)
    {
        try { Log.Debug("AVLU", line); } catch { }
    }
}
