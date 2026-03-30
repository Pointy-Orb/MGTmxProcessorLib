using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
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
            if (
                element.Name == "property"
                && element.Attribute("name").Value == "AboveEntityIndex"
                && int.TryParse(element.Attribute("value").Value, out int index)
            )
            {
                tilemap.aboveEntityIndex = index;
            }
            if (element.Name == "tileset")
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
                if (startingIds.Count == Enum.GetValues(typeof(MetaIDs)).Length)
                {
                    startingIds.Sort((left, right) => right.Item1.CompareTo(left.Item1));
                }
            }
            if (element.Name == "layer")
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
                        if (!int.TryParse(csvNums[j], out id))
                        {
                            continue;
                        }
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
                                        tilemap.mechanicsLayer[j, i] = id - startingIds[l].Item1;
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
        }
        return tilemap;
    }
}
