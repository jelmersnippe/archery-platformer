using Godot;

public partial class ImpactEffectArrow : Arrow {
	[Export] public PackedScene ImpactScene = null!;
	[Export] public CollisionFilter? Filter;
	
	protected override void Impact(KinematicCollision2D collision) {
		if (Filter == null || Filter.Validate(collision)) {
			GD.Print("impact triggered");
			var impact = ImpactScene.Instantiate<Node2D>();
			impact.GlobalPosition = collision.GetPosition();
			GetTree().CurrentScene.CallDeferred("add_child", impact);
		}
		
		QueueFree();
	}
}
