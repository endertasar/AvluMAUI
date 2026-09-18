namespace AvluMAUI.Helpers;

/// <summary>
/// Startup trace: dosyaya + platforma özgü system log'a yazar.
/// Debug console çalışmasa bile okunabilir.
/// </summary>
public static partial class StartupLogger
{
    static string? _path;
    static readonly object _lock = new();

    public static void Init()
    {
        try
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            Directory.CreateDirectory(dir);
            _path = Path.Combine(dir, "startup.log");
            File.WriteAllText(_path, "");
        }
        catch { }

        Write("=== APP START ===");
        try { Write($"OS: {DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString} | {DeviceInfo.Current.Model}"); }
        catch { Write("OS: (DeviceInfo not ready)"); }
    }

    public static void Step(string tag) => Write($"[OK] {tag}");

    public static void Fail(string tag, Exception ex)
    {
        Write($"[ERR] {tag}");
        Write($"      {ex.GetType().Name}: {ex.Message}");
        if (ex.InnerException != null)
            Write($"      inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
        Write(ex.StackTrace ?? "(no stack)");
    }

    public static string? LogPath => _path;

    // ── internal ──────────────────────────────────────────────────────────

    static void Write(string msg)
    {
        var line = $"[{DateTime.Now:HH:mm:ss.fff}] {msg}";

        // 1) Debug output (IDE varsa görünür)
        System.Diagnostics.Debug.WriteLine(line);

        // 2) stderr — xcrun simctl launch --console ile görünür
        Console.Error.WriteLine(line);
        try { Console.Error.Flush(); } catch { }

        // 3) Platform system log (NSLog / Logcat)
        PlatformWrite(line);

        // 4) Dosya (post-mortem okuma için)
        lock (_lock)
        {
            try { if (_path != null) File.AppendAllText(_path, line + "\n"); } catch { }
        }
    }

    // Platform-specific partial — Platforms/iOS ve Platforms/Android klasörlerinde implemente edilir
    static partial void PlatformWrite(string line);
}
