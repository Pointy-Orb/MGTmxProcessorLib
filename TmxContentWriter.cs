using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using PInput = TmxProcessorLib.Tilemap;

namespace TmxProcessorLib;

[ContentTypeWriter]
public class TmxContentWriter : ContentTypeWriter<PInput>
{
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return "TmxLib.TilemapReader, TmxLib";
    }

    protected override void Write(ContentWriter output, PInput value)
    {
        output.Write(value.TilesetName);
        output.Write(value.width);
        output.Write(value.height);
        output.Write(value.tileWidth);
        output.Write(value.tileHeight);
        for (int l = 0; l < value.drawLayers.Count; l++)
        {
            for (int i = 0; i < value.drawLayers[l].GetLength(0); i++)
            {
                for (int j = 0; j < value.drawLayers[l].GetLength(1); j++)
                {
                    if (value.drawLayers[l][i, j] < 0)
                    {
                        continue;
                    }
                    output.Write((sbyte)l);
                    output.Write((byte)i);
                    output.Write((byte)j);
                    output.Write((short)value.drawLayers[l][i, j]);
                    //output.Write(new Vector4(l, i, j, value.drawLayers[l][i, j]));
                }
            }
        }
        for (int i = 0; i < value.collisionLayer.GetLength(0); i++)
        {
            for (int j = 0; j < value.collisionLayer.GetLength(1); j++)
            {
                if (value.collisionLayer[i, j] < 0)
                {
                    continue;
                }
                output.Write((sbyte)-1);
                output.Write((byte)i);
                output.Write((byte)j);
                output.Write((short)value.collisionLayer[i, j]);
                //output.Write(new Vector4(-1, i, j, value.collisionLayer[i, j]));
            }
        }
        for (int i = 0; i < value.mechanicsLayer.GetLength(0); i++)
        {
            for (int j = 0; j < value.mechanicsLayer.GetLength(1); j++)
            {
                if (value.mechanicsLayer[i, j] < 0)
                {
                    continue;
                }
                output.Write((sbyte)-2);
                output.Write((byte)i);
                output.Write((byte)j);
                output.Write((short)value.mechanicsLayer[i, j]);
                //output.Write(new Vector4(-2, i, j, value.mechanicsLayer[i, j]));
            }
        }
        for (int i = 0; i < value.mechanicsLayer.GetLength(0); i++)
        {
            for (int j = 0; j < value.mechanicsLayer.GetLength(1); j++)
            {
                if (value.interactionsLayer[i, j] < 0)
                {
                    continue;
                }
                output.Write((sbyte)-2);
                output.Write((byte)i);
                output.Write((byte)j);
                output.Write((short)value.mechanicsLayer[i, j]);
                //output.Write(new Vector4(-2, i, j, value.mechanicsLayer[i, j]));
            }
        }
        for (int i = 0; i < value.interactionsLayer.GetLength(0); i++)
        {
            for (int j = 0; j < value.interactionsLayer.GetLength(1); j++)
            {
                output.Write((sbyte)-3);
                output.Write((byte)i);
                output.Write((byte)j);
                output.Write((short)value.interactionsLayer[i, j]);
            }
        }
        output.Write((sbyte)-1);
        output.Write(byte.MaxValue);
        output.Write(byte.MaxValue);
        output.Write((short)-1);
        output.Write((short)value.aboveEntityIndex);
    }
}
