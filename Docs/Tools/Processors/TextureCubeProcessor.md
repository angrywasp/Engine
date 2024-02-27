# TextureCubeProcessor

The TextureCubeProcessor takes 6 input images and converts them into a texture cube. Useful for generating skybox and cubemapping textures.

## Input File Types

.png

## Output File Type

.texcube

## Program Arguments

None. The program accepts as input 6 square input files, all the same size and an output file name

### Example Usage

TextureCubeProcessor \<input-front> \<input-back> \<input-up> \<input-down> \<input-left> \<input-right> \<output-file>

## Output File Format

The file format is very simple and consists only of a 4 byte header, detailing the size of the input images.  
This is followed by the raw rgb data of each input image ordered sequentially in the program input list and then gzip compressed.

### File Structure

.texcube header
    - image size

gzip compressed raw rgb image data

## Loading .texcube files

``` C#
static TextureCube Engine.Content.ContentLoader.LoadTextureCube(GraphicsDevice g, string assetName);
```

.texcube files are cached on the first load. Subsequent loadings will return the `TextureCube` instance stored in the cache.