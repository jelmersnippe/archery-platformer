using Godot;

public partial class Arrow : CharacterBody2D {
	[Signal]
	public delegate void TransitionedToStuckEventHandler(Arrow activeArrow, StuckArrow stuckArrow);

	[Signal]
	public delegate void ReleasedEventHandler(Arrow activeArrow);

	[Export] public PackedScene StuckArrowScene = null!;
	[Export] public AnimatedSprite2D Sprite = null!;
	[Export] public CollisionShape2D CollisionShape2D = null!;
	[Export] public float Gravity;

	private bool _collided;
	private bool _released;

	public override void _PhysicsProcess(double delta) {
		if (_collided) {
			return;
		}

		if (!_released) {
			return;
		}

		Velocity += new Vector2(0, Gravity);

		LookAt(Position + Velocity);

		KinematicCollision2D collision = MoveAndCollide(Velocity * (float)delta, true);
		if (collision != null) {
			float spriteOffset = Sprite.SpriteFrames.GetFrameTexture("default", 0).GetSize().X / 4;
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

	private void SpawnStuckArrow() {
		var stuckArrow = StuckArrowScene.Instantiate<Node2D>();
		stuckArrow.Transform = Transform;
		GetParent().CallDeferred("add_child", stuckArrow);
		EmitSignal(SignalName.TransitionedToStuck, this, stuckArrow);
		QueueFree();
	}

	public void Release(Vector2 velocity) {
		Velocity = velocity;
		CollisionShape2D.SetDeferred("disabled", false);
		Reparent(GetTree().CurrentScene);
		EmitSignal(SignalName.Released, this);
		_released = true;
	}
}
