using Godot;

public abstract partial class Arrow : CharacterBody2D {
	[Signal]
	public delegate void ReleasedEventHandler(Arrow activeArrow);
	[Signal]
	public delegate void CancelledEventHandler(Arrow activeArrow);

	[Export] public AnimatedSprite2D Sprite = null!;
	[Export] public CollisionShape2D CollisionShape2D = null!;
	[Export] public float Gravity = 20f;
	[Export] public float StuckArrowLifeTime;
	[Export] public bool TransitionsToStuck = true;
	private PackedScene _stuckArrowScene = ResourceLoader.Load<PackedScene>("res://arrows/scenes/stuck_arrow.tscn");

	private bool _collided;
	private bool _released;
	private bool _validRelease = true;

	public override void _Ready() {
		CollisionShape2D.SetDeferred("disabled", false);
		SetCollisionLayerValue(3, false);
	}

	public override void _PhysicsProcess(double delta) {
		if (_collided) {
			return;
		}

		if (!_released) {
			KinematicCollision2D preReleaseCollision = MoveAndCollide(Transform.X * (float)delta, true);
			if (preReleaseCollision != null) {
				Sprite.Modulate = Colors.Red;
				_validRelease = false;
			}
			else {
				Sprite.Modulate = Colors.White;
				_validRelease = true;
			}
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
			
			if (collision.GetCollider() is Node2D node) {
				Reparent(node);
			}

			Vector2 normal = collision.GetNormal();
			if (TransitionsToStuck) {
				Sprite.AnimationFinished += () => {
					SpawnStuckArrow(Mathf.Abs(normal.X) > Mathf.Abs(normal.Y));
					Impact(collision);
				};
			}
			else {
				Impact(collision);
			}
			QueueFree();
			return;
		}

		MoveAndSlide();
	}

	public void Release(Vector2 velocity) {
		if (!_validRelease) {
			EmitSignal(SignalName.Cancelled, this);
			QueueFree();
			return;
		}
		
		SetCollisionLayerValue(3, true);
		
		Velocity = velocity;
		CollisionShape2D.SetDeferred("disabled", false);
		Reparent(GetTree().CurrentScene);
		EmitSignal(SignalName.Released, this);
		_released = true;
	}

	protected abstract void Impact(KinematicCollision2D collision);
	
	private StuckArrow SpawnStuckArrow(bool isSolid) {
		var stuckArrow = _stuckArrowScene.Instantiate<StuckArrow>();
		stuckArrow.Transform = Transform;
		stuckArrow.Sprite.Texture = Sprite.SpriteFrames.GetFrameTexture(Sprite.Animation, 0);
		
		GetParent().CallDeferred("add_child", stuckArrow);
		GetParent().TreeExiting += () => {
			if (IsInstanceValid(stuckArrow)) {
				stuckArrow.QueueFree();
			}
		};

		if (isSolid) {
			stuckArrow.SetSolid(StuckArrowLifeTime);
		}

		return stuckArrow;
	}
}
