using Godot;

[GlobalClass]
public partial class ArrowType : Resource {
	[Export] public string Name = "ArrowType";
	[Export] public Texture2D DisplaySprite = null!;
	[Export] public PackedScene ArrowScene = null!;
}
