using System.Collections.Generic;

namespace TmxProcessorLib;

public class Tilemap
{
    public string TilesetName;

    public List<int[,]> drawLayers = new();
    public int[,] collisionLayer;
    public int[,] mechanicsLayer;
    public int[,] interactionsLayer;

    public int width;
    public int height;

    public int tileWidth = 16;
    public int tileHeight = 16;

    public int aboveEntityIndex = -1;
}
