using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClasses
{
    [Serializable]
    public class HexTile
    {
        // TileMap data
        public int x, y, z;
        public float rot;

        // Game data
        public HexTileType type;
        public int diceNumber;
        public bool robber;

        public HexTile(int _x, int _y, int _z, float _rot, HexTileType _hexTileType, int _diceNumber, bool _robber)
        {
            x = _x;
            y = _y;
            z = _z;
            type = _hexTileType;
            diceNumber = _diceNumber;
            robber = _robber;
        }
    }

    public struct BoardContainer
    {
        public List<HexTile> hexTiles;

        public BoardContainer(List<HexTile> _hexTiles)
        {
            hexTiles = _hexTiles;
        }
    }
}
