using Godot;

public partial class Player : CharacterBody2D {
	[Signal]
	public delegate void BowEquippedEventHandler(Bow? bow);

	[Signal]
	public delegate void QuiverEquippedEventHandler(Quiver? quiver);

	[ExportCategory("Refs")] [Export] public AnimatedSprite2D Sprite = null!;
	[Export] public Node2D RotationPoint = null!;
	[Export] public Node2D BowOffset = null!;
	[Export] public Area2D GrabArea = null!;

	[ExportCategory("Archery")] [Export] public Bow? Bow;
	[Export] public Quiver? Quiver;

	[ExportCategory("Movement")] [Export] public float AccelerationTime = 0.3f;
	[Export] public float DecelerationTime = 0.2f;
	[Export] public float MaxSpeed = 300.0f;
	[Export] public float AerialAccelerationTime = 0.2f;

	[ExportCategory("Climbing")] [Export] public float ClimbingAccelerationTime = 0.2f;
	[Export] public float ClimbingDecelerationTime = 0.2f;
	[Export] public float ClimbingMaxSpeed = 200.0f;
	[Export] public float MoveToVineCenterSpeed = 150f;

	[ExportCategory("Jumping")] [Export] public float MaxJumpHeight = 128f;
	[Export] public float JumpTimeToPeak = 0.5f;
	[Export] public float JumpTimeToGround = 0.3f;
	[Export] public float JumpDecelerationOnRelease = 0.5f;
	[Export] public float TerminalVelocity = 1000f;

	[Export] public float CoyoteTime = 0.1f;
	[Export] public float InputBufferTime = 0.1f;

	private float JumpVelocity => 2f * MaxJumpHeight / JumpTimeToPeak * -1f;
	private float JumpGravity => -2f * MaxJumpHeight / (JumpTimeToPeak * JumpTimeToPeak) * -1f;
	private float FallGravity => -2f * MaxJumpHeight / (JumpTimeToGround * JumpTimeToGround) * -1f;
	private float Gravity => Velocity.Y < 0f ? JumpGravity : FallGravity;

	private float _remainingCoyoteTime = 0.1f;
	private float _remainingInputBufferTime;

	private bool _isClimbing;

	public override void _Ready() {
		GrabArea.AreaEntered += GrabAreaOnAreaEntered;
		GrabArea.AreaExited += GrabAreaOnAreaExited;
	}

	private void GrabAreaOnAreaExited(Area2D area) {
		switch (area) {
			case Vine vine: {
				if (vine == _vineInRange) {
					_vineInRange = null;
					_isClimbing = false;
				}

				break;
			}
			case Pickup pickup: {
				if (pickup == _pickupInRange) {
					_pickupInRange = null;
				}

				break;
			}
		}
	}

	public void Equip(Bow? bow) {
		if (bow == null) {
			Bow?.QueueFree();
		}
		else {
			BowOffset.AddChild(bow);
		}

		Bow = bow;
		EmitSignal(SignalName.BowEquipped, bow);
	}

	public void Equip(Quiver? quiver) {
		if (quiver == null) {
			Quiver?.QueueFree();
		}
		else {
			AddChild(quiver);
		}

		Quiver = quiver;

		EmitSignal(SignalName.QuiverEquipped, quiver);
		quiver?.NotifyArrowChanges();
		quiver?.NotifyArrowTypeChanged();
	}

	private Vine? _vineInRange;
	private Pickup? _pickupInRange;

	private void GrabAreaOnAreaEntered(Area2D area) {
		if (area is Vine vine) {
			_vineInRange = vine;
		}

		if (area is Pickup pickup) {
			_pickupInRange = pickup;
		}
	}

	private Vector2 _velocity;

	private void CancelArrow() {
		if (Bow?.CurrentArrow != null) {
			Quiver?.ReturnArrow(Bow.CurrentArrow);
		}

		Bow?.CancelArrow();
	}

	private void HandleAir(float delta) {
		if (IsOnFloor() || _isClimbing) {
			return;
		}

		CancelArrow();

		_remainingCoyoteTime -= delta;
		_velocity.Y += Gravity * delta;
		Sprite.Play("jump");

		if (_velocity.Y < 0 && Input.IsActionJustReleased("jump")) {
			_velocity.Y *= JumpDecelerationOnRelease;
		}

		if (Input.IsActionJustPressed("jump")) {
			if (_remainingCoyoteTime > 0) {
				_velocity.Y = JumpVelocity;
			}
			else {
				_remainingInputBufferTime = InputBufferTime;
			}
		}

		_velocity.Y = Mathf.Min(_velocity.Y, TerminalVelocity);

		MoveHorizontal(delta);
		GrabVine();
	}

	private void GrabVine() {
		if (!_isClimbing && _vineInRange != null && Input.IsActionPressed("move_up")) {
			_isClimbing = true;
		}
	}

	private void HandleGrounded(float delta) {
		if (!IsOnFloor()) {
			return;
		}

		_remainingCoyoteTime = CoyoteTime;
		_velocity.Y = 0;

		if (_remainingInputBufferTime > 0) {
			_velocity.Y = JumpVelocity;
		}

		if (Input.IsActionJustPressed("jump")) {
			_velocity.Y = JumpVelocity;
		}

		float horizontalDirection = MoveHorizontal(delta);

		Sprite.Play(horizontalDirection != 0 ? "move" : "idle");

		if (Input.IsActionJustPressed("shoot")) {
			Arrow? arrow = Quiver?.GetArrow();
			if (arrow != null) {
				Bow?.ReadyArrow(arrow);
			}
		}

		if (Input.IsActionJustReleased("shoot")) {
			Bow?.ReleaseArrow();
		}

		GrabVine();
	}

	private void HandleClimbing(float delta) {
		if (!_isClimbing || _vineInRange == null) {
			return;
		}

		CancelArrow();

		_velocity = Vector2.Zero;
		GlobalPosition =
			new Vector2(
				Mathf.MoveToward(GlobalPosition.X, _vineInRange.GlobalPosition.X, MoveToVineCenterSpeed * delta),
				GlobalPosition.Y);

		float direction = Input.GetAxis("move_up", "move_down");

		if (direction != 0) {
			_velocity.Y = Mathf.MoveToward(Velocity.Y, ClimbingMaxSpeed * direction,
				ClimbingMaxSpeed / ClimbingAccelerationTime * delta);
		}
		else {
			_velocity.Y = Mathf.MoveToward(Velocity.Y, 0, ClimbingMaxSpeed / ClimbingDecelerationTime * delta);
		}

		if (Input.IsActionJustPressed("jump")) {
			_velocity.Y = JumpVelocity;
			_isClimbing = false;
		}
	}

	private float MoveHorizontal(float delta) {
		float direction = Input.GetAxis("move_left", "move_right");

		if (direction != 0) {
			_velocity.X = Mathf.MoveToward(Velocity.X, MaxSpeed * direction,
				MaxSpeed / (IsOnFloor() ? AccelerationTime : AerialAccelerationTime) * delta);
			Sprite.FlipH = direction < 0;
		}
		else {
			_velocity.X = Mathf.MoveToward(Velocity.X, 0, MaxSpeed / DecelerationTime * delta);
		}

		return direction;
	}

	public override void _PhysicsProcess(double delta) {
		_velocity = Velocity;
		_remainingInputBufferTime -= (float)delta;

		RotationPoint.Rotation = RotationPoint.GlobalPosition.DirectionTo(GetGlobalMousePosition()).Angle();

		HandleAir((float)delta);
		HandleGrounded((float)delta);
		HandleClimbing((float)delta);

		Velocity = _velocity;
		MoveAndSlide();

		if (Input.IsActionJustPressed("recall")) {
			Quiver?.Recall();
		}

		if (Input.IsActionJustPressed("interact")) {
			_pickupInRange?.Grab(this);
		}

		if (Input.IsActionJustPressed("next_arrow_type")) {
			Quiver?.ChangeArrowType(1);
		}
	}
}
