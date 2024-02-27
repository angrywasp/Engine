# TextureProcessor

The TextureProcessor converts input images into gzip compressed raw rgb pixel values.

## Input File Types

.bmp .png .jpg

## Output File Type

.texture

## Program Arguments

Get a full list by running `TextureProcessor --help`

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

## Loading .texture files

``` C#
static Texture2D Engine.Content.ContentLoader.LoadTexture(GraphicsDevice g, string assetName);
//or to get an additiona list of individual mipmaps as Texture2D objects
static (Texture2D Texture, List<Texture2D> Mipmaps) Engine.Content.ContentLoader.LoadTextureWithMipmaps(GraphicsDevice g, string assetName);
```

.texture files are cached on the first load. Subsequent loadings will return the `Texture2D` instance stored in the cache.
