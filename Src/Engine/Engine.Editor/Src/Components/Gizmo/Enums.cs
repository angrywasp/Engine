namespace Engine.Editor.Components.Gizmo
{
    public enum GizmoAxis
    {
        X,
        Y,
        Z,
        XY,
        ZX,
        YZ,
        None
    }

    public enum GizmoMode
    {
        None,
        Translate,
        Rotate,
        NonUniformScale,
        UniformScale
    }

    public enum TransformSpace
    {
        Local,
        World
    }

    public enum PivotType
    {
        ObjectCenter,
        SelectionCenter,
        WorldOrigin
    }
}