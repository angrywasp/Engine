using Microsoft.Xna.Framework;
using System.Numerics;
using Engine.Content.Terrain;
using System.Threading.Tasks;
using System;
using AngryWasp.Math;
using Engine.World.Objects;

namespace Engine.Editor.Views
{
    //todo: Need update. Terrain only loads with the default texture in this view.
    //Should just render the terrain data as a wireframe
    public class TerrainView : View3D
    {
        public override void InitializeView(string path)
        {
            base.InitializeView(path);

            Vector3 cp = new Vector3(0, 2f, 5f);
			cameraController.Position = cp;
			cameraController.Yaw = 0;
            cameraController.Pitch = -(MathF.Atan2(cp.Y, cp.Z) * Radian.ToDegCoefficient);

            engine.Scene.CreateMapObject<RuntimeMapObject>("Skybox", "DemoContent/Maps/Skyboxes/BlueGreenSkybox.type");

            /*RuntimeMapObject terrain = engine.Scene.CreateMapObjectAsync<RuntimeMapObject>();
            terrain.Loaded += async (mapObject, gameObject) => {
                await terrain.SetGameObject<HeightmapTerrain>(new HeightmapTerrainType
                {
                    HeightMap = path,
                    DiffuseMap = "Engine/Textures/Default_albedo.texture",
                    NormalMap = "Engine/Textures/Default_normal.texture",
                    TextureScale = Vector2.One
                }).ConfigureAwait(false);
			    HeightmapTerrain terrainObject = ((HeightmapTerrain)terrain.GameObject);
                asset = terrainObject;
            };*/
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            engine.Interface.ScreenMessages.WriteStaticText(1, $"Nodes: {QTNode.NodesVisible}", Color.White);
        }
    }
}