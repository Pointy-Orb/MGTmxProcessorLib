using System.Collections.Generic;

namespace TmxProcessorLib;

public class Tilemap
{
    public List<int[,]> drawLayers;
    public int[,] collisionLayer;
    public int[,] interactionLayer;
}
