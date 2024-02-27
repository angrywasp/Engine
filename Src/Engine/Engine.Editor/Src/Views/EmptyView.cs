namespace Engine.Editor.Views
{
    public class EmptyView : View3D
    {
        public override void InitializeView(string path)
        {
            base.InitializeView(path);
            engine.Scene.LoadMap("DemoContent/Maps/EmptyEditorViewMap.map");
            engine.Interface.CreateDefaultForm();
        }
    }
}