using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TheMythalProphecy.Game.Data;

/// <summary>
/// Game settings (audio, video, controls)
/// Persisted to INI file
/// </summary>
public class GameSettings
{
    // Audio settings (default 75%)
    public float MasterVolume { get; set; } = 0.75f;
    public float MusicVolume { get; set; } = 0.75f;
    public float SFXVolume { get; set; } = 0.75f;
    public bool MusicEnabled { get; set; } = true;
    public bool SFXEnabled { get; set; } = true;

    // Video settings
    public int ResolutionWidth { get; set; } = 1280;
    public int ResolutionHeight { get; set; } = 720;
    public bool Fullscreen { get; set; } = false;
    public bool VSync { get; set; } = true;

    // File path for settings
    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TheMythalProphecy",
        "settings.ini"
    );

    /// <summary>
    /// Save settings to INI file
    /// </summary>
    public void Save()
    {
        try
        {
            string directory = Path.GetDirectoryName(SettingsFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var writer = new StreamWriter(SettingsFilePath);

            // Audio section
            writer.WriteLine("[Audio]");
            writer.WriteLine($"MasterVolume={MasterVolume.ToString(CultureInfo.InvariantCulture)}");
            writer.WriteLine($"MusicVolume={MusicVolume.ToString(CultureInfo.InvariantCulture)}");
            writer.WriteLine($"SFXVolume={SFXVolume.ToString(CultureInfo.InvariantCulture)}");
            writer.WriteLine($"MusicEnabled={MusicEnabled}");
            writer.WriteLine($"SFXEnabled={SFXEnabled}");
            writer.WriteLine();

            // Video section
            writer.WriteLine("[Video]");
            writer.WriteLine($"ResolutionWidth={ResolutionWidth}");
            writer.WriteLine($"ResolutionHeight={ResolutionHeight}");
            writer.WriteLine($"Fullscreen={Fullscreen}");
            writer.WriteLine($"VSync={VSync}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Load settings from INI file (or create default if not found)
    /// </summary>
    public static GameSettings Load()
    {
        var settings = new GameSettings();

        try
        {
            if (!File.Exists(SettingsFilePath))
                return settings;

            var values = ParseIniFile(SettingsFilePath);

            // Audio settings
            if (values.TryGetValue("Audio.MasterVolume", out string masterVol))
                settings.MasterVolume = ParseFloat(masterVol, settings.MasterVolume);
            if (values.TryGetValue("Audio.MusicVolume", out string musicVol))
                settings.MusicVolume = ParseFloat(musicVol, settings.MusicVolume);
            if (values.TryGetValue("Audio.SFXVolume", out string sfxVol))
                settings.SFXVolume = ParseFloat(sfxVol, settings.SFXVolume);
            if (values.TryGetValue("Audio.MusicEnabled", out string musicEnabled))
                settings.MusicEnabled = ParseBool(musicEnabled, settings.MusicEnabled);
            if (values.TryGetValue("Audio.SFXEnabled", out string sfxEnabled))
                settings.SFXEnabled = ParseBool(sfxEnabled, settings.SFXEnabled);

            // Video settings
            if (values.TryGetValue("Video.ResolutionWidth", out string resWidth))
                settings.ResolutionWidth = ParseInt(resWidth, settings.ResolutionWidth);
            if (values.TryGetValue("Video.ResolutionHeight", out string resHeight))
                settings.ResolutionHeight = ParseInt(resHeight, settings.ResolutionHeight);
            if (values.TryGetValue("Video.Fullscreen", out string fullscreen))
                settings.Fullscreen = ParseBool(fullscreen, settings.Fullscreen);
            if (values.TryGetValue("Video.VSync", out string vsync))
                settings.VSync = ParseBool(vsync, settings.VSync);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
        }

        return settings;
    }

    private static Dictionary<string, string> ParseIniFile(string path)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string currentSection = "";

        foreach (var line in File.ReadLines(path))
        {
            var trimmed = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(';') || trimmed.StartsWith('#'))
                continue;

            // Section header
            if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                currentSection = trimmed[1..^1];
                continue;
            }

            // Key=Value pair
            int equalsIndex = trimmed.IndexOf('=');
            if (equalsIndex > 0)
            {
                string key = trimmed[..equalsIndex].Trim();
                string value = trimmed[(equalsIndex + 1)..].Trim();
                string fullKey = string.IsNullOrEmpty(currentSection) ? key : $"{currentSection}.{key}";
                values[fullKey] = value;
            }
        }

        return values;
    }

    private static float ParseFloat(string value, float defaultValue)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result)
            ? result : defaultValue;
    }

    private static int ParseInt(string value, int defaultValue)
    {
        return int.TryParse(value, out int result) ? result : defaultValue;
    }

    private static bool ParseBool(string value, bool defaultValue)
    {
        return bool.TryParse(value, out bool result) ? result : defaultValue;
    }

    /// <summary>
    /// Apply audio settings to AudioManager
    /// </summary>
    public void ApplyAudioSettings()
    {
        var audio = Core.GameServices.Audio;
        if (audio != null)
        {
            audio.MasterVolume = MasterVolume;
            audio.MusicVolume = MusicVolume;
        }
    }

    /// <summary>
    /// Apply video settings to GraphicsDeviceManager
    /// </summary>
    public void ApplyVideoSettings()
    {
        var graphics = Core.GameServices.Graphics;
        if (graphics != null)
        {
            graphics.PreferredBackBufferWidth = ResolutionWidth;
            graphics.PreferredBackBufferHeight = ResolutionHeight;
            graphics.IsFullScreen = Fullscreen;
            graphics.ApplyChanges();
        }
    }
}
