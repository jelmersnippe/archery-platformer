using Godot;

public partial class Arrow : CharacterBody2D
{
	[Signal]
	public delegate void TransitionedToStuckEventHandler(Arrow activeArrow, StuckArrow stuckArrow);
	
	[Export] public PackedScene StuckArrowScene;
	[Export] public AnimatedSprite2D Sprite;
	[Export] public CollisionShape2D CollisionShape2D;

	private bool _collided;
	
	public override void _PhysicsProcess(double delta)
	{
		if (_collided) {
			return;
		}
		
		KinematicCollision2D collision = MoveAndCollide(Velocity * (float)delta, true);
		if (collision != null) {
			float spriteOffset = (Sprite.SpriteFrames.GetFrameTexture("default", 0).GetSize().X / 4);
			GlobalPosition = collision.GetPosition() - Velocity.Normalized() * spriteOffset;
			Stick();
			return;
		}
		
		MoveAndSlide();
	}

	public void Stick() {
		if (_collided) {
			return;
		}
		
		Velocity = Vector2.Zero;
		Sprite.Play();
		Sprite.AnimationFinished += SpawnStuckArrow;
		_collided = true;
	}
	
	private void SpawnStuckArrow()
	{
		var stuckArrow = StuckArrowScene.Instantiate<Node2D>();
		stuckArrow.Transform = Transform;
		GetParent().CallDeferred("add_child", stuckArrow);
		EmitSignal(SignalName.TransitionedToStuck, this, stuckArrow);
		QueueFree();
	}
}
