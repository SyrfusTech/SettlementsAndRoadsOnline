using SharedClasses;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    public static void SaveData(List<HexTile> _hexTiles, string _fileName)
    {
        HexTileListContainer hexTileListContainer = new HexTileListContainer(_hexTiles);
        string jsonString = JsonUtility.ToJson(hexTileListContainer);
        string filePath = Application.persistentDataPath + "/" + _fileName + ".board";
        File.WriteAllText(filePath, jsonString);
    }

    public static List<HexTile> LoadData(string _fileName)
    {
        string filePath = Application.persistentDataPath + "/" + _fileName + ".board";
        string jsonString = File.ReadAllText(filePath);
        HexTileListContainer hexTileListContainer = JsonUtility.FromJson<HexTileListContainer>(jsonString);
        return hexTileListContainer.hexTiles;
    }
}