# HeightmapProcessor

The heightmapProcessor converts a greyscale image into vertex/index buffers which can be consumed by the terrain LOD system. HeightmapProcessor includes optional iterative erosion and smoothing to enhance the input heightmap.

## Input File Types

.png .bmp .jpg

## Output File Type

.terrain

## Program Arguments
 
-v: Vertical scale  
-i: Smoothing iteration count  
-e Erosion iteration count  

The following switches are only valid if -e > 0 and control aspects of the iterative erosion

-c: Sediment capacity  
-d: Deposition  
-s: Soil softness

## File Structure

```
- .terrain header
    - Width of heightmap
    - Length of Heightmap
    
- Array of vertical height values
- Array of MeshVertex information
    - Vertex position
    - Vertex texture coordinate
    - Vertex normal
```

## Loading .terrain files

``` C#
static HeightmapData LoadHeightmap(GraphicsDevice g, string assetName);
```
        