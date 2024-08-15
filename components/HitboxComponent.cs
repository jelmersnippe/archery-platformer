using Godot;

public partial class HitboxComponent : Area2D {
	[Signal]
	public delegate void HitEventHandler(Node2D node);

	[Export] public int ContactDamage = 1;
	[Export] public float HitstopTime = 0.02f;

	public override void _Ready() {
		AreaEntered += HandleCollision;
		BodyEntered += HandleCollision;
	}

	private void HandleCollision(Node2D node) {
		EmitSignal(SignalName.Hit, node);
	}

	public bool CanDamage(HurtboxComponent hurtbox) {
		// TODO: Implement logic to track already damaged entities
		return true;
	}
}
