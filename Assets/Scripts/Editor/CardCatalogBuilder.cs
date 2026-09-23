#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RatLab.Data;
using UnityEditor;
using UnityEngine;

public static class CardCatalogBuilder
{
    private const string CardsRoot = "Assets/Cards";
    private const string OutputFolder = "Assets/Resources/Cards";
    private const string OutputPath = OutputFolder + "/CardCatalog.asset";

    [InitializeOnLoadMethod]
    private static void BuildWhenCatalogIsMissing()
    {
        EditorApplication.delayCall += () =>
        {
            if (Directory.Exists(CardsRoot)
                && AssetDatabase.LoadAssetAtPath<CardCatalog>(OutputPath) == null)
            {
                Rebuild();
            }
        };
    }

    [MenuItem("Tools/RatLab/Rebuild Card Catalog")]
    public static void Rebuild()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder(OutputFolder);

        List<CardVariantRecord> records = new List<CardVariantRecord>();
        string[] cardDirectories = Directory.GetDirectories(CardsRoot, "Card*", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (string cardDirectory in cardDirectories)
        {
            string cardId = Path.GetFileName(cardDirectory);
            for (int level = 1; level <= 3; level++)
            {
                CardLevelData levelData = ReadLevel(cardId, cardDirectory, level);
                if (levelData.Image != null)
                {
                    records.Add(new CardVariantRecord(cardId, level, levelData));
                }
            }
        }

        CardCatalog catalog = AssetDatabase.LoadAssetAtPath<CardCatalog>(OutputPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<CardCatalog>();
            AssetDatabase.CreateAsset(catalog, OutputPath);
        }

        catalog.SetData(records, FindEnemyBack());
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"RatLab: catálogo generado con {records.Count} cartas en {OutputPath}.");
    }

    private static CardLevelData ReadLevel(string cardId, string cardDirectory, int level)
    {
        string levelDirectory = Path.Combine(cardDirectory, $"Level{level}").Replace('\\', '/');
        string statsPath = $"{levelDirectory}/CardStats.txt";
        TextAsset stats = AssetDatabase.LoadAssetAtPath<TextAsset>(statsPath);

        if (stats == null)
        {
            throw new InvalidOperationException($"Missing stats file: {statsPath}");
        }

        Dictionary<string, int> values = ParseStats(stats.text, statsPath);
        Sprite sprite = FindSprite(levelDirectory);

        return new CardLevelData(
            sprite,
            values["Norte"],
            values["Este"],
            values["Sur"],
            values["Oeste"]);
    }

    private static Dictionary<string, int> ParseStats(string text, string sourcePath)
    {
        Dictionary<string, int> values = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (string rawLine in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            string[] parts = line.Split(new[] { '=' }, 2);
            if (parts.Length != 2 || !int.TryParse(parts[1].Trim(), out int value) || value < 1 || value > 9)
            {
                throw new InvalidOperationException($"Invalid card value in {sourcePath}: {line}");
            }

            values[parts[0].Trim()] = value;
        }

        string[] requiredKeys = { "Norte", "Este", "Sur", "Oeste" };
        foreach (string key in requiredKeys)
        {
            if (!values.ContainsKey(key))
            {
                throw new InvalidOperationException($"Missing {key} in {sourcePath}");
            }
        }

        return values;
    }

    private static Sprite FindSprite(string folder)
    {
        string[] imagePaths = Directory.GetFiles(folder, "*.png", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(folder, "*.jpg", SearchOption.TopDirectoryOnly))
            .Select(path => path.Replace('\\', '/'))
            .ToArray();

        foreach (string imagePath in imagePaths)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
            if (sprite != null)
            {
                return sprite;
            }
        }

        return null;
    }

    private static Sprite FindEnemyBack()
    {
        return FindSprite("Assets/Cards/Reverse");
    }

    private static void EnsureFolder(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder))
        {
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            string name = Path.GetFileName(folder);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
