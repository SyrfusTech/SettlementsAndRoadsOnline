using SharedClasses;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static string extension = ".board";
    public static void SaveData(List<HexTile> _hexTiles, string _fileName)
    {
        BoardContainer boardContainer = new BoardContainer(_hexTiles);
        string jsonString = JsonUtility.ToJson(boardContainer);
        string filePath = Application.persistentDataPath + "/" + _fileName + extension;
        File.WriteAllText(filePath, jsonString);
    }

    public static List<HexTile> LoadData(string _fileName)
    {
        string filePath = Application.persistentDataPath + "/" + _fileName + extension;
        string jsonString = File.ReadAllText(filePath);
        BoardContainer boardContainer = JsonUtility.FromJson<BoardContainer>(jsonString);
        return boardContainer.hexTiles;
    }

    public static string LoadDataAsJSON(string _fileName)
    {
        string filePath = Application.persistentDataPath + "/" + _fileName + extension;
        return File.ReadAllText(filePath);
    }

    public static string[] GetBoardNames()
    {
        return Directory.GetFiles(Application.persistentDataPath, $"*{extension}");
    }

    public static string GetExtension()
    {
        return extension;
    }

    public static void DeleteBoard(string _fileName)
    {
        File.Delete(Application.persistentDataPath + "/" + _fileName + extension);
    }
}