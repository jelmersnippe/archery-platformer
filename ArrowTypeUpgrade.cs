using Godot;

[GlobalClass]
public partial class ArrowTypeUpgrade : Resource {
	[Export] public Trigger Trigger = null!;
	[Export] public ArrowType ArrowType = null!;
}
