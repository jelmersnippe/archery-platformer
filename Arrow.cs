using Godot;

public partial class Arrow : CharacterBody2D
{
	[Signal]
	public delegate void TransitionedToStuckEventHandler(Arrow self, StuckArrow stuckArrow);
	
	[Export] public PackedScene StuckArrowScene;
	[Export] public AnimatedSprite2D Sprite;

	private bool _collided;
	
	public override void _PhysicsProcess(double delta)
	{
		if (_collided) {
			return;
		}
		
		KinematicCollision2D collision = MoveAndCollide(Velocity * (float)delta, true);
		if (collision != null) {
			var spriteOffset = (Sprite.SpriteFrames.GetFrameTexture("default", 0).GetSize().X / 4);
			GlobalPosition = collision.GetPosition() - Velocity.Normalized() * spriteOffset;
			Velocity = Vector2.Zero;
			Sprite.Play();
			Sprite.AnimationFinished += SpawnStuckArrow;
			_collided = true;
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
