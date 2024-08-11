using Godot;

public abstract partial class Arrow : CharacterBody2D {
	[Signal]
	public delegate void ReleasedEventHandler(Arrow activeArrow);

	[Export] public AnimatedSprite2D Sprite = null!;
	[Export] public CollisionShape2D CollisionShape2D = null!;
	[Export] public float Gravity = 20f;

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
			_collided = true;

			// Move to collision + extra bit of sprite to simulate penetration
			float spriteOffset = Sprite.SpriteFrames.GetFrameTexture("default", 0).GetSize().X / 4;
			GlobalPosition = collision.GetPosition() + Velocity.Normalized() * spriteOffset;

			Velocity = Vector2.Zero;
			Sprite.Play();

			Impact(collision);
			return;
		}

		MoveAndSlide();
	}


	public void Release(Vector2 velocity) {
		Velocity = velocity;
		CollisionShape2D.SetDeferred("disabled", false);
		Reparent(GetTree().CurrentScene);
		EmitSignal(SignalName.Released, this);
		_released = true;
	}

	protected abstract void Impact(KinematicCollision2D collision);
}
