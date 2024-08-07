using Godot;

public partial class Door : StaticBody2D {
	[Export] public Lever Lever;
	[Export] public CollisionShape2D CollisionShape2D;

	public override void _Ready() {
		if (Lever != null) {
			Lever.Flipped += LeverOnFlipped;
		}
	}

	private void LeverOnFlipped(bool isFlipped) {
		CollisionShape2D.SetDeferred("disabled", isFlipped);
		Visible = !isFlipped;
	}
}
