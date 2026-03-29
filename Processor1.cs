using Microsoft.Xna.Framework.Content.Pipeline;
using TInput = System.Xml.Linq.XDocument;
using TOutput = TmxProcessorLib.Tilemap;

namespace TmxProcessorLib;

[ContentProcessor(DisplayName = "Tmx Processor - Pointy Orb")]
public class Processor1 : ContentProcessor<TInput, TOutput>
{
    public override TOutput Process(TInput input, ContentProcessorContext context)
    {
        return default(TOutput);
    }
}
