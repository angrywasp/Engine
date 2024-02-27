using System;
using AngryWasp.Logger;
using Engine.Helpers;
using System.IO;
using Engine.Content;
using Engine.World;
using Engine.World.Objects;

namespace EngineScripting
{
    public static class Map
    {
        private static string assetPath;
        private static ushort uidIter = 0;

        public static Engine.World.Map Create(string path)
        {
            var map = new Engine.World.Map();
            assetPath = path;

            ContentLoader.SaveJson(map, EngineFolders.ContentPathVirtualToReal(path));
            return map;
        }

        public static MapObject AddMapObject(this Engine.World.Map map, string gameObjectPath, string name)
        {
            MapObject mo = new MapObject(gameObjectPath, name, uidIter++);
            map.Objects.Add(mo);
            return mo;
        }

        public static void AddStaticMesh(this Engine.World.Map map, string meshPath)
        {
            map.StaticMeshes.Add(new StaticMesh
            {
                MeshPath = meshPath
            });
        }

        public static void Save(this Engine.World.Map map)
        {
            ContentLoader.SaveJson(map, EngineFolders.ContentPathVirtualToReal(assetPath));
            Log.Instance.SetColor(ConsoleColor.DarkCyan);
            Log.Instance.Write($"Saved '{assetPath}'");
            Log.Instance.SetColor(ConsoleColor.White);
        }

        /*private static MapView view;

        public static MapView View => view;

        public static void Select(string name)
        {
            #region Validate

            if (map == null)
            {
                Log.Instance.WriteWarning("Map == null");
                return;
            }

            MapObject mo = map.FindByName(name);

            if (mo == null)
            {
                Log.Instance.WriteWarning($"Could not find MapObject '{name}'");
                return;
            }

            if (mo == null || view == null)
                return;

            #endregion

            view.Gizmo.Select(new MapViewGizmoSelection
            {
                Tag = mo
            });
        }

        public static void Deselect(string name)
        {
            #region Validate

            if (map == null)
            {
                Log.Instance.WriteWarning("Map == null");
                return;
            }

            MapObject mo = map.FindByName(name);

            if (mo == null)
            {
                Log.Instance.WriteWarning($"Could not find MapObject '{name}'");
                return;
            }

            if (mo == null || view == null)
                return;

            #endregion

            view.Gizmo.Deselect(new MapViewGizmoSelection
            {
                Tag = mo
            });
        }

        public static void DeselectAll()
        {
            if (view == null)
                return;

            view.Gizmo.SelectedItems.Clear();
        }

        public static void SetGizmoMode(GizmoMode mode)
        {
            if (view != null)
                view.Gizmo.ActiveMode = mode;
        }

        public static void SetGizmoSpace(TransformSpace space)
        {
            if (view != null)
                view.Gizmo.ActiveSpace = space;
        }*/
    }
}