namespace AvluMAUI.Helpers;

public static partial class StartupLogger
{
    // Console.Error.WriteLine (base sınıfta zaten çağrılıyor) xcrun simctl
    // launch --console ile görünür — ek NSLog P/Invoke gerekmez.
    static partial void PlatformWrite(string line) { }
}
