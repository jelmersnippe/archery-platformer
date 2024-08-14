using Godot;

public partial class ImpactEffectArrow : Arrow {
	[Export] public PackedScene ImpactScene = null!;
	[Export] public bool DestroyStuckArrow = false;
	[Export] public CollisionFilter? Filter;
	
	protected override void Impact(KinematicCollision2D collision, StuckArrow stuckArrow) {
		if (Filter == null || Filter.Validate(collision)) {
			var impact = ImpactScene.Instantiate<Node2D>();
			impact.GlobalPosition = collision.GetPosition();
			GetTree().CurrentScene.CallDeferred("add_child", impact);
		}

		if (DestroyStuckArrow) {
			stuckArrow.QueueFree();
		}
	}
}
