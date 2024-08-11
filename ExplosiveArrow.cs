using Godot;

public partial class ExplosiveArrow : Arrow {
	protected override void Impact(KinematicCollision2D collision) {
		GD.Print("boom!");
		QueueFree();
	}
}
