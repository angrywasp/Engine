using System.IO;
using AngryWasp.Logger;
using Engine.Editor.Views;
using Engine.Helpers;

namespace Engine.Editor
{
    public static class AssetLoader
    {
        public static void Load(string file)
        {
            EditorGame.Instance.UnloadView();

            string extension = Path.GetExtension(file).Trim('.').ToLower();
            string p = EngineFolders.ContentPathVirtualToReal(file);

            IView view = NodeTypeToView(Engine.Editor.Helpers.GetNodeTypeFromExtension(extension));

            if (view == null)
            {
                Log.Instance.WriteError($"View not implemented for {file}");
                return;
            }

            Log.Instance.Write("Loading view");
            EditorGame.Instance.LoadView(view, file);

            Log.Instance.Write($"View asset {view.AssetPath}");
        }

        private static IView NodeTypeToView(NodeType nt)
        {
            switch (nt)
            {
                case NodeType.Mesh:
                    return new MeshView();
                case NodeType.TextureCube:
                    return new TextureCubeView();
                case NodeType.Texture:
                    return new TextureView();
                case NodeType.Type:
                    return new EntityTypeView();
                case NodeType.Sound:
                    return new SoundEffectView();
                case NodeType.Terrain:
                    return new TerrainView();
                case NodeType.Material:
                    return new MaterialView();
                case NodeType.Font:
                    return new FontView();
                case NodeType.FontPackage:
                    return new FontPackageView();
                case NodeType.Physics:
                    return new PhysicsView();
                case NodeType.Map:
                    return new MapView();
                default:
                    Log.Instance.Write($"View not implemented for {nt}");
                    return null;
            }
        }
    }
}