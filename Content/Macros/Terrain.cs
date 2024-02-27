using System.Numerics;
using Engine.Scripting;
using Engine.World.Objects;
using EngineScripting;

public class Macro_CreateTerrain : IMacro
{
    public void Run()
    {
        HeightmapTerrainType t1 = Entity.Create<HeightmapTerrainType>("DemoContent/Maps/Terrains/Terrain_513.type");
        SetTextures(ref t1);
        t1.BlendTexture = "DemoContent/Maps/Terrains/Heightmap1_513_Blend.texture";
        t1.TextureScale = new Vector2(100, 100);
        t1.HeightMap = "DemoContent/Maps/Terrains/Heightmap1_513.terrain";
        t1.Save();

        HeightmapTerrainType t2 = Entity.Create<HeightmapTerrainType>("DemoContent/Maps/Terrains/Terrain_1025.type");
        SetTextures(ref t2);
        t2.BlendTexture = "DemoContent/Maps/Terrains/Heightmap2_1025_Blend.texture";
        t2.TextureScale = new Vector2(200, 200);
        t2.HeightMap = "DemoContent/Maps/Terrains/Heightmap2_1025.terrain";
        t2.Save();
    }

    private void SetTextures(ref HeightmapTerrainType t)
    {
        var baseTexturePath = "DemoContent/Materials/Textures";
        t.DiffuseMap = $"{baseTexturePath}/MixedMoss/MixedMoss_albedo.texture";
        t.NormalMap = $"{baseTexturePath}/MixedMoss/MixedMoss_normal.texture";

        t.DiffuseMap0 = $"{baseTexturePath}/RiverRocks/RiverRocks_albedo.texture";
        t.NormalMap0 = $"{baseTexturePath}/RiverRocks/RiverRocks_normal.texture";

        t.DiffuseMap1 = $"{baseTexturePath}/DryDirt2/DryDirt2_albedo.texture";
        t.NormalMap1 = $"{baseTexturePath}/DryDirt2/DryDirt2_normal.texture";
        
        t.DiffuseMap2 = $"{baseTexturePath}/RockyWornGround/RockyWornGround_albedo.texture";
        t.NormalMap2 = $"{baseTexturePath}/RockyWornGround/RockyWornGround_normal.texture";

        t.DiffuseMap3 = $"{baseTexturePath}/Octostone/Octostone_albedo.texture";
        t.NormalMap3 = $"{baseTexturePath}/Octostone/Octostone_normal.texture";
    }
}
