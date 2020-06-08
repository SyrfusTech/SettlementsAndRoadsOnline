using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedClasses;
using UnityEngine.Tilemaps;
using System;

public class HexagonGridEditManager : MonoBehaviour, IClickableObject
{
    public Tilemap hexTileMap;
    public Tilemap diceNumberTileMap;
    public Tilemap robberTileMap;
    public TileBase[] hexTileBases;
    public TileBase[] diceNumberTileBases;
    public TileBase robberTileBase;
    public Grid hexGrid;
    public List<HexTile> hexTiles;

    private enum CurrentPlacement
    {
        HexTile = 0,
        DiceNumber,
        Robber,
        Erase
    }

    private HexTileType hexPlacementType = HexTileType.Desert;
    private int diceNumberPlacementType = 2;
    private CurrentPlacement currentPlacement = CurrentPlacement.HexTile;

    public void ClearAllTiles()
    {
        for (int i = 0; i < hexTiles.Count; i++)
        {
            Vector3Int coords = new Vector3Int(hexTiles[i].x, hexTiles[i].y, hexTiles[i].z);
            hexTileMap.SetTile(coords, null);
            diceNumberTileMap.SetTile(coords, null);
            robberTileMap.SetTile(coords, null);
        }
        hexTiles = new List<HexTile>();
    }

    public void LoadBoard(List<HexTile> _hexTiles)
    {
        hexTiles = _hexTiles;
        for (int i = 0; i < hexTiles.Count; i++)
        {
            Vector3Int coords = new Vector3Int(hexTiles[i].x, hexTiles[i].y, hexTiles[i].z);
            hexTileMap.SetTile(coords, hexTileBases[(int)hexTiles[i].type]);
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, hexTiles[i].rot), Vector3.one);
            hexTileMap.SetTransformMatrix(coords, matrix);
            if (hexTiles[i].diceNumber != 0)
                diceNumberTileMap.SetTile(coords, diceNumberTileBases[hexTiles[i].diceNumber]);
            if (hexTiles[i].robber)
                robberTileMap.SetTile(coords, robberTileBase);
        }
    }

    public void SetHexPlacementType(int _type)
    {
        hexPlacementType = (HexTileType)_type;
        currentPlacement = CurrentPlacement.HexTile;
    }

    public void SetDiceNumberPlacementType(int _type)
    {
        diceNumberPlacementType = _type;
        currentPlacement = CurrentPlacement.DiceNumber;
    }

    public void SetRobberPlacement()
    {
        currentPlacement = CurrentPlacement.Robber;
    }

    public void SetErasePlacement()
    {
        currentPlacement = CurrentPlacement.Erase;
    }

    public void OnLMBClick(Vector3 _worldMousePosition)
    {
        Vector3Int coords = hexGrid.LocalToCell(_worldMousePosition / hexGrid.transform.localScale.x);
        switch (currentPlacement)
        {
            case CurrentPlacement.HexTile:
                {
                    TileBase tile = hexTileMap.GetTile(coords);
                    if (tile == null)
                        AddHexTileAtCoords(coords);
                    else
                        ChangeHexTileAtCoords(coords);
                }
                break;
            case CurrentPlacement.DiceNumber:
                {
                    if (IsDiceNumberPlaceable(coords))
                        SetDiceNumberTileAtCoords(coords);
                }
                break;
            case CurrentPlacement.Robber:
                {
                    if (IsRobberPlaceable(coords))
                        MoveRobberToCoords(coords);
                }
                break;
            case CurrentPlacement.Erase:
                if (hexTileMap.GetTile(coords) != null ||
                    diceNumberTileMap.GetTile(coords) != null ||
                    robberTileMap.GetTile(coords) != null)
                    RemoveTilesAtCoords(coords);
                break;
            default:

                break;
        }
    }

    public void OnRMBClick(Vector3 _worldMousePosition)
    {
        Vector3Int coords = hexGrid.LocalToCell(_worldMousePosition / hexGrid.transform.localScale.x);
        TileBase tile = hexTileMap.GetTile(coords);
        if (tile != null)
        {
            RotateTileAtCoords(coords);
        }
    }

    private void AddHexTileAtCoords(Vector3Int _coords)
    {
        hexTileMap.SetTile(_coords, hexTileBases[(int)hexPlacementType]);
        hexTiles.Add(new HexTile(_coords.x, _coords.y, _coords.z, 0.0f, hexPlacementType, 0, false));
    }

    private void ChangeHexTileAtCoords(Vector3Int _coords)
    {
        hexTileMap.SetTile(_coords, hexTileBases[(int)hexPlacementType]);
        int i = GetListIndexFromCoords(_coords);
        hexTiles[i].type = hexPlacementType;
    }

    private void SetDiceNumberTileAtCoords(Vector3Int _coords)
    {
        diceNumberTileMap.SetTile(_coords, diceNumberTileBases[diceNumberPlacementType]);
        int i = GetListIndexFromCoords(_coords);
        hexTiles[i].diceNumber = diceNumberPlacementType;
    }

    private void MoveRobberToCoords(Vector3Int _coords)
    {
        int i = GetListIndexOfRobber();
        if (i != -1)
        {
            hexTiles[i].robber = false;
            robberTileMap.SetTile(new Vector3Int(hexTiles[i].x, hexTiles[i].y, hexTiles[i].z), null);
        }
        robberTileMap.SetTile(_coords, robberTileBase);
        i = GetListIndexFromCoords(_coords);
        hexTiles[i].robber = true;
    }

    private void RemoveTilesAtCoords(Vector3Int _coords)
    {
        int i = GetListIndexFromCoords(_coords);
        hexTiles.RemoveAt(i);
        hexTileMap.SetTile(_coords, null);
        diceNumberTileMap.SetTile(_coords, null);
        robberTileMap.SetTile(_coords, null);
    }

    private int GetListIndexFromCoords(Vector3Int _coords)
    {
        for (int i = 0; i < hexTiles.Count; i++)
        {
            if (hexTiles[i].x == _coords.x && hexTiles[i].y == _coords.y && hexTiles[i].z == _coords.z)
                return i;
        }
        return -1;
    }

    private int GetListIndexOfRobber()
    {
        for (int i = 0; i < hexTiles.Count; i++)
        {
            if (hexTiles[i].robber)
                return i;
        }
        return -1;
    }

    private void RotateTileAtCoords(Vector3Int _coords)
    {
        int i = GetListIndexFromCoords(_coords);
        float rot = hexTiles[i].rot;
        rot = SyrfusMath.Mod(rot + 60.0f, 360.0f);
        hexTiles[i].rot = rot;
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, rot), Vector3.one);
        hexTileMap.SetTransformMatrix(_coords, matrix);
    }

    private bool IsDiceNumberPlaceable(Vector3Int _coords)
    {
        TileBase hexTile = hexTileMap.GetTile(_coords);
        if (hexTile == null)
            return false;
        int i = GetListIndexFromCoords(_coords);
        return (hexTiles[i].type == HexTileType.ClayMine ||
            hexTiles[i].type == HexTileType.SheepGrass ||
            hexTiles[i].type == HexTileType.RockMountain ||
            hexTiles[i].type == HexTileType.WheatField ||
            hexTiles[i].type == HexTileType.WoodForest);
    }

    private bool IsRobberPlaceable(Vector3Int _coords)
    {
        TileBase hexTile = hexTileMap.GetTile(_coords);
        if (hexTile == null)
            return false;
        int i = GetListIndexFromCoords(_coords);
        return (hexTiles[i].type == HexTileType.ClayMine ||
            hexTiles[i].type == HexTileType.SheepGrass ||
            hexTiles[i].type == HexTileType.RockMountain ||
            hexTiles[i].type == HexTileType.WheatField ||
            hexTiles[i].type == HexTileType.WoodForest ||
            hexTiles[i].type == HexTileType.Desert);
    }

    private bool HasNeighbor(Vector3Int _coords)
    {
        Vector3Int[] directions;
        if (SyrfusMath.Mod(_coords.y, 2) == 0)
            directions = GetDirectionsEven(_coords);
        else
            directions = GetDirectionsOdd(_coords);

        foreach (Vector3Int direction in directions)
        {
            if (hexTileMap.HasTile(direction))
                return true;
        }
        return false;
    }

    private Vector3Int[] GetDirectionsEven(Vector3Int _coords)
    {
        Vector3Int[] directions = new Vector3Int[6];
        directions[0] = new Vector3Int(_coords.x - 1, _coords.y + 1, _coords.z); // North West
        directions[1] = new Vector3Int(_coords.x, _coords.y + 1, _coords.z);     // North East
        directions[2] = new Vector3Int(_coords.x + 1, _coords.y, _coords.z);     // East
        directions[3] = new Vector3Int(_coords.x, _coords.y - 1, _coords.z);     // South East
        directions[4] = new Vector3Int(_coords.x - 1, _coords.y - 1, _coords.z); // South West
        directions[5] = new Vector3Int(_coords.x - 1, _coords.y, _coords.z);     // West
        return directions;
    }

    private Vector3Int[] GetDirectionsOdd(Vector3Int _coords)
    {
        Vector3Int[] directions = new Vector3Int[6];
        directions[0] = new Vector3Int(_coords.x, _coords.y + 1, _coords.z);     // North West
        directions[1] = new Vector3Int(_coords.x + 1, _coords.y + 1, _coords.z); // North East
        directions[2] = new Vector3Int(_coords.x + 1, _coords.y, _coords.z);     // East
        directions[3] = new Vector3Int(_coords.x + 1, _coords.y - 1, _coords.z); // South East
        directions[4] = new Vector3Int(_coords.x, _coords.y - 1, _coords.z);     // South West
        directions[5] = new Vector3Int(_coords.x - 1, _coords.y, _coords.z);     // West
        return directions;
    }
}
