namespace SkiaSpineSharp;

public class SkiaSpineTextureLoader : TextureLoader
{
    public void Load(AtlasPage page, string path)
    {
        SKBitmap bitmap = SKBitmap.Decode(path);

        page.rendererObject = bitmap;
    }

    public void Unload(object texture)
    {
        (texture as SKBitmap)?.Dispose();
    }
}
