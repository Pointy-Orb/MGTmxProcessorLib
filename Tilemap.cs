using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TmxProcessorLib;

public class Tilemap
{
    public string TilesetName;
    public string InteractionsKey;

    public List<int[,]> drawLayers = new();
    public int[,] collisionLayer;
    public int[,] mechanicsLayer;
    public int[,] interactionsLayer;

    public Dictionary<Vector3, RotationData> rotationData = new();
    internal List<EnemySpawner> enemySpawners = new();

    public int width;
    public int height;

    public int tileWidth = 16;
    public int tileHeight = 16;

    public int aboveEntityIndex = -1;
}

public struct RotationData
{
    public bool horizontalFlip;
    public bool verticalFlip;
    public bool diagonalFlip;

    public RotationData(bool verticalFlip, bool horizontalFlip, bool diagonalFlip)
    {
        this.verticalFlip = verticalFlip;
        this.horizontalFlip = horizontalFlip;
        this.diagonalFlip = diagonalFlip;
    }
}
