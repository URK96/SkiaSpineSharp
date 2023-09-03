namespace SkiaSpineSharp;

public class SkiaSpineRenderer
{
    public int[] QuadTriangles { get; set; } = { 0, 1, 2, 1, 3, 2 };

    public SKCanvas Draw(SKCanvas canvas, Skeleton skeleton)
    {
        canvas.Save();

        foreach (Slot slot in skeleton.DrawOrder)
        {
            Attachment attachment = slot.Attachment;

            if (attachment is null)
            {
                continue;
            }

            SKBitmap texture = null;
            AtlasRegion region = null;
            float[] worldVertices = new float[8];
            float[] textureUVs = Array.Empty<float>();
            int[] triangles = Array.Empty<int>();

            if (attachment is RegionAttachment regionAttachment)
            {
                region = regionAttachment.RendererObject as AtlasRegion;
                texture = region.page.rendererObject as SKBitmap;
                textureUVs = regionAttachment.UVs;

                regionAttachment.ComputeWorldVertices(slot.Bone, worldVertices);

                triangles = QuadTriangles;
            }
            else if (attachment is MeshAttachment meshAttachment)
            {
                region = meshAttachment.RendererObject as AtlasRegion;
                texture = region.page.rendererObject as SKBitmap;
                textureUVs = meshAttachment.UVs;

                if (worldVertices.Length < textureUVs.Length)
                {
                    worldVertices = new float[textureUVs.Length];
                }

                meshAttachment.ComputeWorldVertices(slot, worldVertices);

                triangles = meshAttachment.Triangles;
            }
            else if (attachment is SkinnedMeshAttachment skinnedMeshAttachment)
            {
                region = skinnedMeshAttachment.RendererObject as AtlasRegion;
                texture = region.page.rendererObject as SKBitmap;
                textureUVs = skinnedMeshAttachment.UVs;

                if (worldVertices.Length < textureUVs.Length)
                {
                    worldVertices = new float[textureUVs.Length];
                }

                skinnedMeshAttachment.ComputeWorldVertices(slot, worldVertices);

                triangles = skinnedMeshAttachment.Triangles;
            }
            else
            {
                continue;
            }

            if (texture is null)
            {
                continue;
            }

            int textureWidth = texture.Width;
            int textureHeight = texture.Height;
            List<SKPoint> vertices = new();
            List<SKPoint> texturePoints = new();
            ushort[] indicies = triangles.Select(x => (ushort)x).ToArray();

            for (int i = 0; i < worldVertices.Length; i += 2)
            {
                vertices.Add(new SKPoint(worldVertices[i], worldVertices[i + 1]));
                texturePoints.Add(new SKPoint(textureWidth * textureUVs[i], textureHeight * textureUVs[i + 1]));
            }

            using SKPaint paint = new()
            {
                Shader = texture.ToShader(),
                FilterQuality = SKFilterQuality.Medium
            };

            canvas.DrawVertices(
                SKVertexMode.Triangles,
                vertices.ToArray(),
                texturePoints.ToArray(),
                null,
                indicies,
                paint);
        }

        canvas.Restore();

        return canvas;
    }
}
