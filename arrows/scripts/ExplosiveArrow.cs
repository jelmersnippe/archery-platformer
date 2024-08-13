using Godot;

public partial class ExplosiveArrow : Arrow {
	[Export] public PackedScene ExplosionScene = null!;
	
	protected override void Impact(KinematicCollision2D collision) {
		var explosion = ExplosionScene.Instantiate<Explosion>();
		explosion.GlobalPosition = collision.GetPosition();
		GetTree().CurrentScene.CallDeferred("add_child", explosion);
		
		QueueFree();
	}
}
