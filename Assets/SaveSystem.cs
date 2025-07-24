using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static readonly string SAVE_FILE = "/rankings.json";

    public static void SaveRankings(RankingsData data)
    {
        string path = Application.persistentDataPath + SAVE_FILE;
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
    }

    public static RankingsData LoadRankings()
    {
        string path = Application.persistentDataPath + SAVE_FILE;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<RankingsData>(json);
        }
        // Si no existe el archivo, devuelve una lista nueva y vacía.
        return new RankingsData();
    }
}