using Godot;
using System.Linq;

public partial class Explosion : Area2D {
	[Export] public AnimationPlayer AnimationPlayer = null!;
	[Export] public float KnockbackForce = 700f;
	
	public override void _Ready() {
		AnimationPlayer.AnimationFinished += name => QueueFree();
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body) {
		Knockable? knockableNode = body.GetChildren().OfType<Knockable>().FirstOrDefault();

		knockableNode?.ApplyKnockback(GlobalPosition.DirectionTo(body.GlobalPosition) * KnockbackForce);
	}
}
