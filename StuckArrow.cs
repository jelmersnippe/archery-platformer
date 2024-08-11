using Godot;

public partial class StuckArrow : StaticBody2D {
	[Signal]
	public delegate void LifeTimeRanOutEventHandler();

	[Export] public Area2D PlayerDetectionArea = null!;
	[Export] public CollisionShape2D CollisionShape = null!;
	[Export] public CollisionShape2D PlayerDetectionShape = null!;

	private float _remainingLifeTime;
	private Player? _trackingPlayer;

	private bool _destroyOnContact = true;

	public override void _Ready() {
		PlayerDetectionArea.BodyEntered += PlayerDetectionAreaOnAreaEntered;
		PlayerDetectionArea.BodyExited += PlayerDetectionAreaOnBodyExited;
	}

	public void SetSolid(float lifeTime) {
		_remainingLifeTime = lifeTime;
		// Always detect player when solid so arrow can break
		PlayerDetectionShape.SetDeferred("disabled", false);

		float normalizedRotation = (GlobalRotationDegrees + 360) % 360;

		if (!(lifeTime > 0f)) {
			return;
		}

		// Collision for jumping is disabled by default, otherwise player can jump once with
		// input buffering time before touching the arrow. SO when it has lifetime we enable
		CollisionShape.SetDeferred("disabled", false);
		if (normalizedRotation >= 90f && normalizedRotation <= 270f) {
			CollisionShape.RotationDegrees = 180f;
		}
	}

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

	public override void _Process(double delta) {
		// If we are tracking the player and the player is not moving up
		if (_trackingPlayer == null || _trackingPlayer.Velocity.Y < 0) {
			return;
		}

		// Player is standing on the arrow
		if (_trackingPlayer.Velocity.Y == 0) {
			_remainingLifeTime -= (float)delta;
		}

		if (_remainingLifeTime <= 0f) {
			EmitSignal(SignalName.LifeTimeRanOut);
			QueueFree();
		}
	}
}
