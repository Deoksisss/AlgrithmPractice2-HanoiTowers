using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace HanoiTowers.Core;

public class LevelRecord
{
    public int LevelNumber { get; set; }
    public int DiskCount { get; set; }
    public bool IsUnlocked { get; set; }
    public int Stars { get; set; }
    public int BestMoves { get; set; }
    public float BestTimeSeconds { get; set; }
}

public class SaveData
{
    public int HighestUnlockedLevel { get; set; } = 1;
    public Dictionary<int, LevelRecord> Levels { get; set; } = new();
    public Dictionary<int, int> FreePlayBestMoves { get; set; } = new();
    public Dictionary<int, int> FreePlayBestStars { get; set; } = new();
    public bool SoundEnabled { get; set; } = true;
}

public static class GameProgress
{
    private const string SavePath = "user://hanoi_save_data.json";
    public static SaveData Current { get; private set; } = new();

    static GameProgress()
    {
        InitializeDefault();
        Load();
    }

    private static void InitializeDefault()
    {
        Current = new SaveData { HighestUnlockedLevel = 1 };
        for (int i = 1; i <= 8; i++)
        {
            Current.Levels[i] = new LevelRecord
            {
                LevelNumber = i,
                DiskCount = i + 2, // 1 -> 3, 2 -> 4, ..., 8 -> 10
                IsUnlocked = (i == 1),
                Stars = 0,
                BestMoves = int.MaxValue,
                BestTimeSeconds = 0f
            };
        }
    }

    public static void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Current, options);
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            if (file != null)
            {
                file.StoreString(json);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to save game progress: {ex.Message}");
        }
    }

    public static void Load()
    {
        try
        {
            if (!FileAccess.FileExists(SavePath))
            {
                Save();
                return;
            }

            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            if (file != null)
            {
                string json = file.GetAsText();
                var data = JsonSerializer.Deserialize<SaveData>(json);
                if (data != null)
                {
                    Current = data;
                    // Ensure all 8 levels exist
                    for (int i = 1; i <= 8; i++)
                    {
                        if (!Current.Levels.ContainsKey(i))
                        {
                            Current.Levels[i] = new LevelRecord
                            {
                                LevelNumber = i,
                                DiskCount = i + 2,
                                IsUnlocked = (i <= Current.HighestUnlockedLevel),
                                Stars = 0,
                                BestMoves = int.MaxValue
                            };
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to load game progress, resetting: {ex.Message}");
            InitializeDefault();
        }
    }

    public static void RecordLevelResult(int levelNumber, int moves, float timeSeconds)
    {
        if (!Current.Levels.TryGetValue(levelNumber, out var record)) return;

        int diskCount = record.DiskCount;
        int stars = StarCalculator.CalculateStars(moves, diskCount);

        if (stars > record.Stars)
        {
            record.Stars = stars;
        }

        if (moves < record.BestMoves)
        {
            record.BestMoves = moves;
        }

        if (record.BestTimeSeconds <= 0f || timeSeconds < record.BestTimeSeconds)
        {
            record.BestTimeSeconds = timeSeconds;
        }

        // Unlock next level if available
        int nextLevel = levelNumber + 1;
        if (nextLevel <= 8)
        {
            if (nextLevel > Current.HighestUnlockedLevel)
            {
                Current.HighestUnlockedLevel = nextLevel;
            }
            if (Current.Levels.TryGetValue(nextLevel, out var nextRecord))
            {
                nextRecord.IsUnlocked = true;
            }
        }

        Save();
    }

    public static void RecordFreePlayResult(int diskCount, int moves)
    {
        int stars = StarCalculator.CalculateStars(moves, diskCount);
        if (!Current.FreePlayBestStars.TryGetValue(diskCount, out int currentStars) || stars > currentStars)
        {
            Current.FreePlayBestStars[diskCount] = stars;
        }

        if (!Current.FreePlayBestMoves.TryGetValue(diskCount, out int bestMoves) || moves < bestMoves)
        {
            Current.FreePlayBestMoves[diskCount] = moves;
        }

        Save();
    }
}
