using Godot;

public abstract partial class Arrow : CharacterBody2D {
	[Signal]
	public delegate void ReleasedEventHandler(Arrow arrow);

	[Signal]
	public delegate void CancelledEventHandler(Arrow arrow);

	[Signal]
	public delegate void ImpactedEventHandler(Arrow arrow);

	[Export] public AnimatedSprite2D Sprite = null!;
	[Export] public CollisionShape2D CollisionShape2D = null!;
	[Export] public CollisionShape2D HitboxCollision = null!;

	[Export] public Area2D PlayerDetectionArea = null!;
	[Export] public CollisionShape2D PlayerDetectionShape = null!;

	[Export] public float Gravity = 20f;
	[Export] public float StuckLifeTime;

	private bool _collided;
	private bool _released;
	private bool _validRelease = true;

	private float _remainingLifeTime;
	private Player? _trackingPlayer;

	private enum State {
		Drawn,
		Airborne,
		Impacted
	}

	private State _currentState = State.Drawn;

	public override void _Ready() {
		CollisionShape2D.SetDeferred("disabled", false);
		HitboxCollision.SetDeferred("disabled", true);
		PlayerDetectionShape.SetDeferred("disabled", true);

		PlayerDetectionArea.BodyEntered += PlayerDetectionAreaOnAreaEntered;
		PlayerDetectionArea.BodyExited += PlayerDetectionAreaOnBodyExited;
	}

	private void SetState(State state) {
		if (state == _currentState) {
			return;
		}

		switch (state) {
			case State.Airborne:
				break;
			case State.Impacted:
				break;
		}

		_currentState = state;
	}

	public override void _PhysicsProcess(double delta) {
		switch (_currentState) {
			case State.Drawn:
				KinematicCollision2D preReleaseCollision = MoveAndCollide(Transform.X * (float)delta, true);
				if (preReleaseCollision != null) {
					Sprite.Modulate = Colors.Red;
					_validRelease = false;
				}
				else {
					Sprite.Modulate = Colors.White;
					_validRelease = true;
				}

				break;
			case State.Airborne:
				Velocity += new Vector2(0, Gravity);

				LookAt(Position + Velocity);

				KinematicCollision2D collision = MoveAndCollide(Velocity * (float)delta, true);
				if (collision != null) {
					Stick(collision);
					return;
				}

				MoveAndSlide();
				break;
			case State.Impacted:
				// If we are tracking the player and the player is not moving up
				if (_trackingPlayer == null || _trackingPlayer.Velocity.Y < 0) {
					return;
				}

				// Player is standing on the arrow
				if (_trackingPlayer.Velocity.Y == 0) {
					_remainingLifeTime -= (float)delta;
				}

				if (_remainingLifeTime <= 0f) {
					QueueFree();
				}

				break;
		}
	}

	public void Release(Vector2 velocity) {
		if (!_validRelease) {
			EmitSignal(SignalName.Cancelled, this);
			QueueFree();
			return;
		}

		Velocity = velocity;
		SetCollisionLayerValue(3, true);
		CollisionShape2D.SetDeferred("disabled", false);
		HitboxCollision.SetDeferred("disabled", false);
		Reparent(GetTree().CurrentScene);
		EmitSignal(SignalName.Released, this);
		_currentState = State.Airborne;
	}

	private void Stick(KinematicCollision2D collision) {
		HitboxCollision.SetDeferred("disabled", true);
		PlayerDetectionShape.SetDeferred("disabled", false);
		CollisionShape2D.OneWayCollision = true;
		float normalizedRotation = (GlobalRotationDegrees + 360) % 360;
		if (normalizedRotation >= 90f && normalizedRotation <= 270f) {
			CollisionShape2D.RotationDegrees = 180f;
		}

		// Move to collision + extra bit of sprite to simulate penetration
		float spriteOffset = Sprite.SpriteFrames.GetFrameTexture("default", 0).GetSize().X / 4;
		GlobalPosition = collision.GetPosition() + Velocity.Normalized() * spriteOffset;

		Velocity = Vector2.Zero;
		Sprite.Play();

		if (collision.GetCollider() is Node2D node) {
			Reparent(node);
		}

		_remainingLifeTime = StuckLifeTime;
		SetCollisionLayerValue(3, false);
		SetCollisionLayerValue(6, true);
		_currentState = State.Impacted;
		EmitSignal(SignalName.Impacted, this);
		Impact(collision);
	}

	protected abstract void Impact(KinematicCollision2D collision);

	private void PlayerDetectionAreaOnBodyExited(Node2D body) {
		if (body == _trackingPlayer) {
			_trackingPlayer = null;
		}
	}

	private void PlayerDetectionAreaOnAreaEntered(Node2D area) {
		if (area is not Player player) {
			return;
		}

		_trackingPlayer = player;
	}
}
