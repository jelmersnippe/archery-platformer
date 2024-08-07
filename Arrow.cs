using Godot;

public partial class Arrow : CharacterBody2D
{
	[Signal]
	public delegate void TransitionedToStuckEventHandler(Arrow self, StuckArrow stuckArrow);
	
	[Export] public PackedScene StuckArrowScene;
	[Export] public AnimatedSprite2D Sprite;
	
	public override void _PhysicsProcess(double delta)
	{
		var collision = MoveAndCollide(Velocity * (float)delta, true);
		if (collision != null)
		{
			Velocity = Vector2.Zero;
			Sprite.Play();
			Sprite.AnimationFinished += SpawnStuckArrow;
			return;
		}
		
		MoveAndSlide();
	}

	private void SpawnStuckArrow()
	{
			var stuckArrow = StuckArrowScene.Instantiate<Node2D>();
			stuckArrow.Transform = Transform;
			GetParent().AddChild(stuckArrow);
			EmitSignal(SignalName.TransitionedToStuck, this, stuckArrow);
			QueueFree();
	}
}
