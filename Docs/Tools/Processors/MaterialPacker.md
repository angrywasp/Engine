# MaterialPacker

The MaterialPacker packs textures into the correect channels for use in the material system.

## Input File Types

.bmp .png .jpg

## Output File Type

*_albedo.texture
*_normal.texture
*_pbr.texture
*_emissive.texture

## Program Arguments

Get a full list by running `MaterialPacker --help`

## File Structure

```
Header
    - width
    - height
    - channel count
    - mipmap count

if (Mipmaps count > 0)
    for (mm in mipmaps)
        - mm width
        - mm height
        - mm Texture data
else
    - Texture data
```

## Material Texture Channels

```
Albedo (32-bit RGBA)
    - RGB: Albedo color
    - A: Alpha mask
Normal (24-bit RGB)
    - RGB: Normal
PBR (32-bit RGBA)
    - R: Metalness
    - G: AO
    - B: Roughness
    - A: Displacement
Emissive (32-bit RGBA)
    - RGB: Emissive color
    - A: Emissive intensity
```

## Loading .texture files

``` C#
static Texture2D Engine.Content.ContentLoader.LoadTexture(GraphicsDevice g, string assetName);
//or to get an additiona list of individual mipmaps as Texture2D objects
static (Texture2D Texture, List<Texture2D> Mipmaps) Engine.Content.ContentLoader.LoadTextureWithMipmaps(GraphicsDevice g, string assetName);
```

.texture files are cached on the first load. Subsequent loadings will return the `Texture2D` instance stored in the cache.
