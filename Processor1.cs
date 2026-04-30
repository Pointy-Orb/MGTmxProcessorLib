using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using TInput = System.Xml.Linq.XDocument;
using TOutput = TmxProcessorLib.Tilemap;

namespace TmxProcessorLib;

internal enum MetaIDs
{
    Collision,
    Mechanics,
    Interactions,
}

[ContentProcessor(DisplayName = "Tmx Processor - Pointy Orb")]
public class Processor1 : ContentProcessor<TInput, TOutput>
{
    public override TOutput Process(TInput input, ContentProcessorContext context)
    {
        var tilemap = new Tilemap();
        List<(int, MetaIDs)> startingIds = new();
        foreach (var node in input.DescendantNodes())
        {
            if (!(node is XElement element))
            {
                continue;
            }
            if (element.Name == "map")
            {
                ProcessMap(element, tilemap);
            }

            if (
                element.Name == "property"
                && element.Attribute("name").Value == "AboveEntityIndex"
                && int.TryParse(element.Attribute("value").Value, out int index)
            )
            {
                tilemap.aboveEntityIndex = index;
            }
            if (element.Name == "property" && element.Attribute("name").Value == "InteractionsKey")
            {
                tilemap.InteractionsKey = element.Attribute("value").Value;
            }
            if (element.Name == "property" && element.Attribute("name").Value == "Unflippable")
            {
                tilemap.Unflippable = element.Attribute("value").Value == "true";
            }

            if (element.Name == "tileset")
            {
                ProcessTileset(element, tilemap, startingIds);
            }
            if (element.Name == "layer")
            {
                ProcessLayer(tilemap, element, startingIds);
            }
            if (element.Name == "object")
            {
                if (element.Attribute("type").Value == "EnemySpawner")
                {
                    ProcessEnemySpawner(element, tilemap);
                }
            }
        }
        return tilemap;
    }

    private void ProcessTileset(XElement element, Tilemap tilemap, List<(int, MetaIDs)> startingIds)
    {
        var source = element.Attribute("source");
        if (source.Value.Contains("Collisions"))
        {
            if (int.TryParse(element.Attribute("firstgid").Value, out int firstGid))
            {
                startingIds.Add((firstGid, MetaIDs.Collision));
            }
        }
        else if (source.Value.Contains("Mechanics"))
        {
            if (int.TryParse(element.Attribute("firstgid").Value, out int firstGid))
            {
                startingIds.Add((firstGid, MetaIDs.Mechanics));
            }
        }
        else if (source.Value.Contains("Interactions"))
        {
            if (int.TryParse(element.Attribute("firstgid").Value, out int firstGid))
            {
                startingIds.Add((firstGid, MetaIDs.Interactions));
            }
        }
        else
        {
            var backslashIndex = source.Value.LastIndexOf('/');
            var dotIndex = source.Value.LastIndexOf('.');
            var prunedName = source.Value.Substring(
                backslashIndex + 1,
                dotIndex - backslashIndex - 1
            );
            tilemap.TilesetName = prunedName;
        }
        startingIds.Sort((left, right) => right.Item1.CompareTo(left.Item1));
    }

    private void ProcessLayer(Tilemap tilemap, XElement element, List<(int, MetaIDs)> startingIds)
    {
        int[,] drawLayer = new int[tilemap.width, tilemap.height];
        string csv = "";
        foreach (var descendant in element.Descendants())
        {
            if (descendant.Name == "data")
            {
                csv = descendant.Value;
            }
        }
        csv = csv.Trim('\n');
        string[] csvLines = csv.Split('\n');
        bool layerIsntEmpty = false;
        for (int i = 0; i < csvLines.Length; i++)
        {
            csvLines[i] = csvLines[i].Trim();
            csvLines[i] = csvLines[i].TrimEnd(',');
            string[] csvNums = csvLines[i].Split(',');
            for (int j = 0; j < csvNums.Length; j++)
            {
                int id;
                uint rawID;
                if (!UInt32.TryParse(csvNums[j], out rawID))
                {
                    continue;
                }
                id = (int)(rawID << 3);
                id = id >> 3;
                bool metaID = false;
                for (int l = 0; l < startingIds.Count; l++)
                {
                    if (id >= startingIds[l].Item1)
                    {
                        switch (startingIds[l].Item2)
                        {
                            case MetaIDs.Collision:
                                tilemap.collisionLayer[j, i] = id - startingIds[l].Item1;
                                break;
                            case MetaIDs.Mechanics:
                                int mechanicID = id - startingIds[l].Item1;
                                tilemap.mechanicsLayer[j, i] = mechanicID;
                                break;
                            case MetaIDs.Interactions:
                                tilemap.interactionsLayer[j, i] = id - startingIds[l].Item1;
                                break;
                        }
                        metaID = true;
                        break;
                    }
                }
                if (metaID)
                {
                    continue;
                }
                bool horizontalFlip = (rawID >> 31 & 1) == 1;
                bool verticalFlip = (rawID >> 30 & 1) == 1;
                bool diagonalFlip = (rawID >> 29 & 1) == 1;
                if (verticalFlip || horizontalFlip || diagonalFlip)
                {
                    tilemap.rotationData.Add(
                        new Vector3(tilemap.drawLayers.Count, j, i),
                        new RotationData(verticalFlip, horizontalFlip, diagonalFlip)
                    );
                }

                drawLayer[j, i] = id - 1;
                if (id > 0)
                {
                    layerIsntEmpty = true;
                }
            }
        }
        if (layerIsntEmpty)
        {
            tilemap.drawLayers.Add(drawLayer);
        }
    }

    private void ProcessMap(XElement element, Tilemap tilemap)
    {
        var width = element.Attribute("width");
        if (int.TryParse(width.Value, out int widthVar))
        {
            tilemap.width = widthVar;
        }
        var height = element.Attribute("height");
        if (int.TryParse(height.Value, out int heightVar))
        {
            tilemap.height = heightVar;
        }
        tilemap.collisionLayer = new int[tilemap.width, tilemap.height];
        tilemap.mechanicsLayer = new int[tilemap.width, tilemap.height];
        tilemap.interactionsLayer = new int[tilemap.width, tilemap.height];
        for (int i = 0; i < tilemap.width; i++)
        {
            for (int j = 0; j < tilemap.height; j++)
            {
                tilemap.collisionLayer[i, j] = -1;
                tilemap.mechanicsLayer[i, j] = -1;
                tilemap.interactionsLayer[i, j] = -1;
            }
        }

        var tilewidth = element.Attribute("tilewidth");
        if (int.TryParse(tilewidth.Value, out int tilewidthVar))
        {
            tilemap.tileWidth = tilewidthVar;
        }
        var tileheight = element.Attribute("tileheight");
        if (int.TryParse(tileheight.Value, out int tileheightVar))
        {
            tilemap.tileHeight = tileheightVar;
        }
    }

    private void ProcessEnemySpawner(XElement element, Tilemap tilemap)
    {
        List<WorldEnemyBlueprint> enemies = new();
        foreach (var descendant in element.Descendants("item"))
        {
            if (
                descendant.Attribute("type").Value != "class"
                || descendant.Attribute("propertytype").Value != "WorldEnemy"
            )
            {
                continue;
            }
            enemies.Add(ProcessWorldEnemy(descendant, tilemap));
        }

        ushort? attemptInterval = null;
        ushort? attemptSuccessFraction = null;
        foreach (var property in element.Descendants("property"))
        {
            switch (property.Attribute("name").Value)
            {
                case "Attempt Interval":
                    if (UInt16.TryParse(property.Attribute("value").Value, out var interval))
                    {
                        attemptInterval = interval;
                    }
                    break;
                case "Attempt Success Fraction":
                    if (UInt16.TryParse(property.Attribute("value").Value, out var fraction))
                    {
                        attemptSuccessFraction = fraction;
                    }
                    break;
            }
        }

        //Tiled won't write any data if the data is at its default value, so we have to recreate the default if there is no data
        if (enemies.Count <= 0)
        {
            enemies.Add(new WorldEnemyBlueprint(0, new List<ushort>() { 0 }));
        }

        ushort x = GetNumFromAttribute("x", element) ?? 0;
        ushort y = GetNumFromAttribute("y", element) ?? 0;
        ushort width = GetNumFromAttribute("width", element) ?? 1;
        ushort height = GetNumFromAttribute("height", element) ?? 1;

        var spawner = new EnemySpawner(
            x,
            y,
            width,
            height,
            enemies,
            attemptInterval,
            attemptSuccessFraction
        );

        tilemap.enemySpawners.Add(spawner);
    }

    private WorldEnemyBlueprint ProcessWorldEnemy(XElement element, Tilemap tilemap)
    {
        List<ushort> enemies = new();
        ushort type = 0;

        XAttribute valueAttribute;
        foreach (XElement property in element.Descendants("property"))
        {
            string name = property.Attribute("name").Value;
            if (
                name == "Type"
                && (valueAttribute = property.Attribute("value")) != null
                && UInt16.TryParse(valueAttribute.Value, out var typeValue)
            )
            {
                type = typeValue;
            }
            if (name == "Battle Enemies")
            {
                foreach (XElement item in property.Descendants("item"))
                {
                    var enemyId = GetNumFromAttribute("value", item);
                    if (enemyId != null)
                    {
                        enemies.Add((ushort)enemyId);
                    }
                }
            }
        }

        if (enemies.Count <= 0)
        {
            enemies.Add(0);
        }
        return new WorldEnemyBlueprint(type, enemies);
    }

    private ushort? GetNumFromAttribute(string attributeName, XElement element)
    {
        if (element.Attribute(attributeName) == null)
        {
            return null;
        }
        if (UInt16.TryParse(element.Attribute(attributeName).Value, out var elementValue))
        {
            return elementValue;
        }
        if (Single.TryParse(element.Attribute(attributeName).Value, out var floatValue))
        {
            return (ushort)floatValue;
        }
        return null;
    }
}
