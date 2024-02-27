using System.IO;
using AngryWasp.Logger;
using Engine.Content.Terrain;
using Engine.Interfaces;
using Engine.Helpers;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Engine.UI;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Engine.Serializers;
using SkiaSharp;
using System.Threading.Tasks;
using Engine.AssetTransport;
using Engine.Graphics.Effects.Builder;
using Engine.Graphics.Materials;
using Engine.World;
using Engine.Content.Model;
using System.Diagnostics;
using Engine.Content.Model.Instance;
using Engine.Content.Model.Template;
using System.Numerics;
using BepuPhysics.Collidables;
using System.Runtime.InteropServices;
using AngryWasp.Helpers;
using Microsoft.Xna.Framework.Audio;

namespace Engine.Content
{
    public static class ContentLoader
    {
        private static Dictionary<int, Texture2D> textureCache = new Dictionary<int, Texture2D>();
        private static Dictionary<int, TextureCube> textureCubeCache = new Dictionary<int, TextureCube>();
        private static Dictionary<int, FontPackage> fontCache = new Dictionary<int, FontPackage>();
        private static Dictionary<int, (MeshTemplate Mesh, SharedMeshData SharedData)> meshCache = new Dictionary<int, (MeshTemplate Mesh, SharedMeshData SharedData)>();
        private static Dictionary<int, MeshMaterial> materialCache = new Dictionary<int, MeshMaterial>();
        private static Dictionary<int, SoundEffect> soundCache = new Dictionary<int, SoundEffect>();

        private static AsyncLock textureLock = new AsyncLock();
        private static AsyncLock textureCubeLock = new AsyncLock();
        private static AsyncLock meshLock = new AsyncLock();
        private static AsyncLock soundLock = new AsyncLock();

        public static T LoadJson<T>(string assetName) where T : IJsonSerialized
        {
            Log.Instance.Write($"LoadJson: {assetName}");
            assetName = EngineFolders.ContentPathVirtualToReal(assetName);
            var result = JsonConvert.DeserializeObject<T>(File.ReadAllText(assetName), DefaultJsonSerializerOptions());
            return result;
        }

        public static void SaveJson(object asset, string assetName)
        {
            Log.Instance.Write($"SaveJson: {assetName}");
            assetName = EngineFolders.ContentPathVirtualToReal(assetName);
            string jsonString = JsonConvert.SerializeObject(asset, DefaultJsonSerializerOptions());
            File.WriteAllText(assetName, jsonString);
        }

        public static JsonSerializerSettings DefaultJsonSerializerOptions()
        {
            var options = new JsonSerializerSettings();
            options.TypeNameHandling = TypeNameHandling.Objects;
            options.Formatting = Formatting.Indented;
            options.NullValueHandling = NullValueHandling.Include;
            options.ObjectCreationHandling = ObjectCreationHandling.Replace;

            options.Converters.Add(new BoundingBoxSerializer());
            options.Converters.Add(new ColorSerializer());
            options.Converters.Add(new DegreeSerializer());
            options.Converters.Add(new RadianSerializer());
            options.Converters.Add(new MatrixSerializer());
            options.Converters.Add(new QuaternionSerializer());
            options.Converters.Add(new RectangleSerializer());
            options.Converters.Add(new Vector2iSerializer());
            options.Converters.Add(new Vector2Serializer());
            options.Converters.Add(new Vector3Serializer());
            options.Converters.Add(new Vector4Serializer());
            options.Converters.Add(new Transform1Serializer());
            options.Converters.Add(new Transform2Serializer());
            options.Converters.Add(new Transform3Serializer());

            return options;
        }

        public static MeshMaterial LoadMaterial(string virtualPath, bool skinned)
        {
            int key = virtualPath.GetHashCode() * (skinned.GetHashCode() + 1);

            if (key == 0)
                Debugger.Break();

            if (!materialCache.ContainsKey(key))
            {
                Log.Instance.Write($"LoadMaterial: {virtualPath}");

                var b = MeshMaterialReader.Read(File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal(virtualPath)));

                materialCache.Add(key, new MeshMaterial
                {
                    AlbedoMap = b.Albedo,
                    NormalMap = b.Normal,
                    PbrMap = b.Pbr,
                    EmissiveMap = b.Emissive,
                    TextureScale = b.TextureScale,
                    DoubleSided = b.DoubleSided
                });
            }

            return skinned ? MeshUtils.ShallowCopy<SkinnedMeshMaterial>(materialCache[key]) : MeshUtils.ShallowCopy<MeshMaterial>(materialCache[key]);
        }

        public static async Task<MeshInstance> LoadMeshAsync(EngineCore e, string assetName)
        {
            var asyncLock = await meshLock.LockAsync().ConfigureAwait(false);

            int key = assetName.GetHashCode();
            if (!meshCache.ContainsKey(key))
            {
                Log.Instance.Write($"LoadMesh: {assetName}");
                var mesh = MeshReader.Read(File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal(assetName)));
                var buffers = mesh.Type == Mesh_Type.Skinned ? 
                    new SharedMeshData(e.GraphicsDevice, mesh.Positions, mesh.Normals, mesh.SkinWeights, mesh.Indices) :
                    new SharedMeshData(e.GraphicsDevice, mesh.Positions, mesh.Normals, mesh.Indices);
                meshCache.Add(key, (mesh, buffers));
            }

            var cached = meshCache[key];

            var instance = new MeshInstance(key, cached.Mesh, cached.SharedData);
            await instance.LoadAsync(e).ConfigureAwait(false);

            asyncLock.Dispose();

            return instance;
        }

        public static Mesh LoadCollisionMesh(EngineCore e, string assetName)
        {
            //todo: cache

            var data = File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal(assetName));
            var triangles = MemoryMarshal.Cast<byte, Triangle>(data);
            e.Scene.Physics.DefaultBufferPool.Take<Triangle>(triangles.Length, out var buffer);
            buffer.CopyFrom(triangles, 0, 0, triangles.Length);
            
            var shape = new Mesh(buffer, Vector3.One, e.Scene.Physics.DefaultBufferPool);
            return shape;
        }

        public static T LoadShader<T>(GraphicsDevice g, string programPath, string[] includeDirs = null) where T : Shader, new() =>
            ShaderBuilder.BuildFromFile<T>(g, EngineFolders.ContentPathVirtualToReal(programPath), includeDirs);

        public static Texture2D LoadTexture(GraphicsDevice g, string virtualPath)
        {
            int key = virtualPath.GetHashCode();
            if (textureCache.ContainsKey(key))
                return textureCache[key];
            else
            {
                Log.Instance.Write($"LoadShader {virtualPath}");
                var tex = Texture2DReader.Read(g, File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal(virtualPath)));
                textureCache.Add(key, tex);
                return tex;
            }
        }

        
        public static async Task<Texture2D> LoadTextureAsync(GraphicsDevice g, string virtualPath)
        {
            var asyncLock = await textureLock.LockAsync().ConfigureAwait(false);

            int key = virtualPath.GetHashCode();
            if (!textureCache.ContainsKey(key))
            {
                Log.Instance.Write($"LoadTexture: {virtualPath}");
                var tex = await Texture2DReader.ReadAsync(g, File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal(virtualPath))).ConfigureAwait(false);
                textureCache.Add(key, tex);
            }

            asyncLock.Dispose();
            return textureCache[key];
        }

        public static Texture2D LoadSvg(GraphicsDevice g, string assetName, Vector2i size)
        {
            Log.Instance.Write($"LoadSvg: {assetName}");

            assetName = EngineFolders.ContentPathVirtualToReal(assetName);

            if (!File.Exists(assetName))
            {
                Log.Instance.WriteError($"Asset '{assetName}' does not exist");
                return null;
            }

            var sf = size.ToVector2();

            var svg = new SkiaSharp.Extended.Svg.SKSvg(new SKSize(sf.X, sf.Y));
            svg.Load(assetName);

            SKMatrix mat = SKMatrix.CreateScale(sf.X / svg.CanvasSize.Width, sf.Y / svg.CanvasSize.Height);

            using (var skBitmap = new SKBitmap(size.X, size.Y, SKColorType.Rgba8888, SKAlphaType.Premul))
            using (SKCanvas canvas = new SKCanvas(skBitmap))
            {
                canvas.DrawPicture(svg.Picture, ref mat);
                canvas.Flush();
                canvas.Save();

                var t = new Texture2D(g, size.X, size.Y, false, SurfaceFormat.Rgba);
                t.SetData(skBitmap.Bytes);
                return t;
            }
        }

        public static async Task<TextureCube> LoadTextureCubeAsync(GraphicsDevice g, string virtualPath)
        {
            var asyncLock = await textureCubeLock.LockAsync().ConfigureAwait(false);

            int key = virtualPath.GetHashCode();
            if (!textureCubeCache.ContainsKey(key))
            {
                Log.Instance.Write($"LoadTextureCube: {virtualPath}");
                var tex = await TextureCubeReader.ReadAsync(g, File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal(virtualPath))).ConfigureAwait(false);
                textureCubeCache.Add(key, tex);
            }

            asyncLock.Dispose();
            return textureCubeCache[key];
        }

        public static HeightmapData LoadHeightmap(GraphicsDevice g, string assetName)
        {
            Log.Instance.Write($"LoadHeightmap: {assetName}");
            return HeightmapReader.Read(g, File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal(assetName)));
        }

        /*public static TerrainModel LoadGeomipmapTerrain(GraphicsDevice g, string assetName)
		{
			try
			{
				Log.Instance.Write($"LoadGeomipmapTerrain: {assetName}");
				//assetName = FormatFileName(assetName, ".terrain");
				assetName = EngineFolders.ContentPathVirtualToReal(assetName);

				FileStream fs = new FileStream(assetName, FileMode.Open, FileAccess.Read);
				BinaryReader br = new BinaryReader(fs);

				int numPatchesX = BitShifter.ToInt(br.ReadBytes(4));
				int numPatchesY = BitShifter.ToInt(br.ReadBytes(4));

				Patch[,] patches = new Patch[numPatchesX, numPatchesY];

				#region read patch data;

				for (int y = 0; y < numPatchesY; y++)
					for (int x = 0; x < numPatchesX; x++)
					{
						int vertexCount = BitShifter.ToInt(br.ReadBytes(4));
						VertexBuffer vb = new VertexBuffer(g, MeshVertex.Declaration, vertexCount, BufferUsage.None);
						vb.SetData(br.ReadBytes(MeshVertex.VERTEX_SIZE * vertexCount));
						MeshVertex[] m = new MeshVertex[256];
						vb.GetData<MeshVertex>(0, m, 0, 256, MeshVertex.VERTEX_SIZE);

						int levelCount = BitShifter.ToInt(br.ReadBytes(4));
						Level[] levels = new Level[levelCount];

						#region read levels for each patch

						for (int i = 0; i < levelCount; i++)
						{
							int indicesCount = BitShifter.ToInt(br.ReadBytes(4));
							IndexBuffer[] indexBuffers = new IndexBuffer[indicesCount];

							int stride = sizeof(uint);

							for (int j = 0; j < indicesCount; j++)
							{
								int indexBufferLength = BitShifter.ToInt(br.ReadBytes(4));
								IndexBuffer ib = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, indexBufferLength, BufferUsage.None);
								ib.SetData(br.ReadBytes(stride * indexBufferLength));
								indexBuffers[j] = ib;
							}

							float maximumDelta = BitShifter.ToFloat(br.ReadBytes(4));
							levels[i] = new Level(indexBuffers, maximumDelta);
						}

						#endregion

						BoundingBox bb = new BoundingBox(BitShifter.ToVector3(br.ReadBytes(12)), BitShifter.ToVector3(br.ReadBytes(12)));
						Vector3 center = BitShifter.ToVector3(br.ReadBytes(12));
						Vector2 offset = BitShifter.ToVector2(br.ReadBytes(8));

						patches[x, y] = new Patch(vb, levels, bb, center, offset);
					}

				#endregion

				Func<int, int, Patch> getPatch = (x, y) =>
				{
					if (x < 0 || x > numPatchesX - 1 || y < 0 || y > numPatchesY - 1)
						return null;
					return patches[x, y];
				};

				// Now set patch neighbours.
				for (int y = 0; y < numPatchesY; y++)
					for (int x = 0; x < numPatchesX; x++)
						patches[x, y].SetNeighbours(
							getPatch(x - 1, y),
							getPatch(x + 1, y),
							getPatch(x, y - 1),
							getPatch(x, y + 1));

				//read height map
				int w = BitShifter.ToInt(br.ReadBytes(4));
				int h = BitShifter.ToInt(br.ReadBytes(4));
				float[,] values = new float[w, h];

				for (int y = 0; y < h; ++y)
					for (int x = 0; x < w; ++x)
						values[x, y] = BitShifter.ToFloat(br.ReadBytes(4));

				float horizontalScale = BitShifter.ToInt(br.ReadBytes(4));

				HeightData heightmap = new HeightData(w, h, values, horizontalScale);

				br.Close();
				br.Dispose();

				return new TerrainModel(numPatchesX, numPatchesY, patches, heightmap);
			}
			catch (Exception e)
			{
				throw Log.Instance.WriteFatalException(e, "ContentLoader.LoadTerrain failed to load " + assetName);
			}
		}*/

        public static async Task<SoundEffect> LoadSoundEffectAsync(string virtualPath)
        {
            var asyncLock = await soundLock.LockAsync().ConfigureAwait(false);
            int key = virtualPath.GetHashCode();

            if (!soundCache.ContainsKey(key))
            {
                Log.Instance.Write($"LoadSoundEffect: {virtualPath}");
                var snd = SoundReader.Read(File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal(virtualPath)));
                var soundEffect = new SoundEffect(snd.AudioData, snd.SampleRate, AudioChannels.Mono);
                soundCache.Add(key, soundEffect);
            }

            asyncLock.Dispose();
            return soundCache[key];
        }

        public static FontPackage LoadFontPackage(GraphicsDevice g, string assetName)
        {
            int key = assetName.GetHashCode();
            if (fontCache.ContainsKey(key))
                return fontCache[key];

            Log.Instance.Write($"LoadFontPackage: {assetName}");
            var fp = FontPackageReader.Read(g, File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal(assetName)));
            fontCache.Add(key, fp);
            return fp;
        }

        public static Font LoadFont(GraphicsDevice g, string assetName)
        {
            Log.Instance.Write($"LoadFont: {assetName}");
            return FontReader.Read(g, File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal(assetName)));
        }

        public static Map LoadMap(EngineCore e, string assetName)
        {
            Log.Instance.Write($"LoadMap: {assetName}");

            assetName = EngineFolders.ContentPathVirtualToReal(assetName);
            var map = JsonConvert.DeserializeObject<Map>(File.ReadAllText(assetName), DefaultJsonSerializerOptions());

            return map;
        }
    }
}
