# MeshProcessor

The MeshProcessor turns various 3d mesh formats into an engine ready mesh file.

## Input File Types

.gltf
.glb

## Output File Type

.mesh

## Program Arguments

-f: Flip vertex winding  
-c: Compress meshes. If enabled, seperate meshes in the source file will be converted to submeshes and the the processor will output a single file.  
    If not enabled,  each mesh will be exported as a seperate file

*NOTE: The compress meshes option (-c) does not need to be provided when running MeshProcessor. Individual meshes can be merged into a single output*  
*in a second step using the [MeshMerge](../PostProcessors/MeshMerge.md) tool*

### Example Usage

MeshProcessor \<input-file> \<output-file> \<arguments>

The following example would create an inverted cube mesh for use as a skybox.

MeshProcessor Cube.gltf Cube.mesh -f

## Output File Format

There is currently no binary file format for meshes and they instead use the XML format provided by the default serializer. 

### File Structure

The file structure is standard XML