using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;
using TImport = System.Xml.Linq.XDocument;

namespace TmxProcessorLib;

[ContentImporter(
    ".tmx",
    DisplayName = "Tmx Importer - Pointy Orb",
    DefaultProcessor = nameof(Processor1)
)]
public class Importer1 : ContentImporter<TImport>
{
    public override TImport Import(string filename, ContentImporterContext context)
    {
        ThrowIfInvalid(filename);
        var text = File.ReadAllText(filename);
        var document = XDocument.Parse(text);
        return document;
    }

    private void ThrowIfInvalid(string filename)
    {
        using var reader = new XmlTextReader(filename);
        bool hasMap = false;
        bool hasTileset = false;
        bool hasLayer = false;
        try
        {
            while (reader.Read())
            {
                if (reader.LocalName == "map")
                {
                    hasMap = true;
                }
                if (reader.LocalName == "tileset")
                {
                    hasTileset = true;
                }
                if (reader.LocalName == "layer")
                {
                    hasLayer = true;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidContentException(
                "The XML provided appears to be either invalid or malformed. See inner exception.",
                ex
            );
        }
        if (!hasLayer || !hasTileset || !hasMap)
        {
            throw new InvalidContentException(
                $"A necessary component of the .tmx file was not found: \nMap: {hasMap}\nTileset: {hasTileset} \nLayer: {hasLayer}"
            );
        }
    }
}
