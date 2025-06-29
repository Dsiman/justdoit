using Sandbox;
using Editor;

[EditorHandle("materials/gizmo/handle_square.png")]
[Title("MapGen")]
[Category("Tools")]
public sealed class MapGen : Component
{
    [Property, Range(8, 256)] public int Width { get; set; } = 64;
    [Property, Range(8, 256)] public int Height { get; set; } = 64;
    [Property, Range(1f, 100f)] public float Scale { get; set; } = 20f;
    [Property, Range(1f, 200f)] public float HeightMultiplier { get; set; } = 32f;
}