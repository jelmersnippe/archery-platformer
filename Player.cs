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
	[Export] public Area2D InteractableArea = null!;

	[ExportCategory("Archery")] [Export] public Bow? Bow;
	[Export] public Quiver? Quiver;
	[Export] public bool ArcheryUsableWhenNotGrounded = true;
	[Export] public float DrawHorizontalSlowdown = 0.4f;
	[Export] public float DrawVerticalSlowdown = 0.6f;

	[ExportCategory("Movement")] [Export] public float AccelerationTime = 0.3f;
	[Export] public float DecelerationTime = 0.2f;
	[Export] public float MaxSpeed = 300.0f;
	[Export] public float AerialAccelerationTime = 0.6f;
	[Export] public float AerialDecelerationTime = 0.5f;

	[ExportCategory("Vine Climbing")] [Export]
	public float ClimbingAccelerationTime = 0.2f;

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

	[ExportCategory("Wall Climbing")] [Export]
	public float WallClimbGraceTime = 0.2f;

	[Export] public float WallJumpGraceTime = 0.1f;
	[Export] public float WallSlideSpeed = 40f;

	private bool _canWallJump;
	private float _remainingWallClimbGraceTime;
	private float _remainingWallJumpGraceTime;

	private float JumpVelocity => 2f * MaxJumpHeight / JumpTimeToPeak * -1f;
	private float JumpGravity => -2f * MaxJumpHeight / (JumpTimeToPeak * JumpTimeToPeak) * -1f;
	private float FallGravity => -2f * MaxJumpHeight / (JumpTimeToGround * JumpTimeToGround) * -1f;
	private float Gravity => Velocity.Y < 0f ? JumpGravity : FallGravity;

	private float _remainingCoyoteTime = 0.1f;
	private float _remainingInputBufferTime;

	private bool _isClimbing;

	private enum MovementState {
		Grounded,
		Airborne,
		WallGrab,
		WallSlide,
		Climbing
	}

	private MovementState _currentState = MovementState.Grounded;

	[ExportCategory("KillZone")] [Export] public float KillZoneControlLossTime = 0.5f;
	[Export] public float TimeBetweenGroundedPositionTracking = 2f;
	private Vector2 _lastGroundedPosition;
	private float _timeSinceLastGroundedPosition;

	public override void _Ready() {
		GrabArea.AreaEntered += GrabAreaOnAreaEntered;
		GrabArea.AreaExited += GrabAreaOnAreaExited;

		InteractableArea.AreaEntered += InteractableAreaOnAreaEntered;
		InteractableArea.AreaExited += InteractableAreaOnAreaExited;
		_lastGroundedPosition = GlobalPosition;
	}

	private void InteractableAreaOnAreaExited(Area2D area) {
		if (area == _interactableInRange) {
			_interactableInRange.ShowInteractable(false);
			_interactableInRange = null;
		}
	}

	private void GrabAreaOnAreaExited(Area2D area) {
		if (area == _vineInRange) {
			_vineInRange = null;
			_isClimbing = false;
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
			if (Quiver != null) {
				Quiver.QueueFree();
				Quiver.ArrowTypeChanged -= QuiverOnArrowTypeChanged;
			}
		}
		else {
			AddChild(quiver);
			quiver.ArrowTypeChanged += QuiverOnArrowTypeChanged;

			EmitSignal(SignalName.QuiverEquipped, quiver);

			quiver?.NotifyArrowChanges();
			quiver?.NotifyArrowTypeChanged();
		}

		Quiver = quiver;
	}

	private void QuiverOnArrowTypeChanged(ArrowType? arrowType) {
		CancelArrow();
	}

	private Node2D? _vineInRange;
	private Interactable? _interactableInRange;

	private void GrabAreaOnAreaEntered(Area2D area) {
		_vineInRange = area;
	}

	private void InteractableAreaOnAreaEntered(Area2D area) {
		if (area is Interactable interactable) {
			_interactableInRange?.ShowInteractable(false);
			_interactableInRange = interactable;
			_interactableInRange.ShowInteractable(true);
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

		float directionX = MoveHorizontal(delta);
		float wallNormal = GetWallNormal().X;
		bool isPerformingWallClimbInput =
			_velocity.Y >= 0 && IsOnWall() && Mathf.Sign(directionX) == -Mathf.Sign(wallNormal);
		if (isPerformingWallClimbInput) {
			_currentState = MovementState.WallGrab;
		}
	}

	private void HandleWallGrab(float delta) {
		float directionX = MoveHorizontal(delta);

		float wallNormal = GetWallNormal().X;
		bool isPerformingWallClimbInput =
			_velocity.Y >= 0 && IsOnWall() && Mathf.Sign(directionX) == -Mathf.Sign(wallNormal);

		if (!isPerformingWallClimbInput && (_remainingWallClimbGraceTime <= 0 || _remainingWallJumpGraceTime <= 0)) {
			if (_remainingWallClimbGraceTime <= 0) {
				_knownWallNormalX = wallNormal;
				_currentState = MovementState.WallSlide;
			}
			else {
				_currentState = MovementState.Airborne;
			}

			return;
		}

		if (isPerformingWallClimbInput) {
			_remainingWallClimbGraceTime = WallClimbGraceTime;
			_remainingWallJumpGraceTime = WallJumpGraceTime;
		}

		_velocity = Vector2.Zero;

		bool holdingOppositeDirection = Mathf.Sign(directionX) == Mathf.Sign(wallNormal);
		if (holdingOppositeDirection) {
			_remainingWallJumpGraceTime -= delta;
		}

		_remainingWallClimbGraceTime -= delta;

		if (_canWallJump && Input.IsActionJustPressed("jump")) {
			_velocity.Y = JumpVelocity;
			_velocity.X = Mathf.Sign(wallNormal) * MaxSpeed;
			_canWallJump = false;
			Sprite.Modulate = Colors.Red;
			_currentState = MovementState.Airborne;
		}
	}

	private void GrabVine() {
		if (!_isClimbing && _vineInRange != null && Input.IsActionPressed("move_up")) {
			_isClimbing = true;
		}
	}

	private void HandleGrounded(float delta) {
		if (!IsOnFloor()) {
			_currentState = MovementState.Airborne;
			return;
		}

		float horizontalDirection = MoveHorizontal(delta);

		Sprite.Play(horizontalDirection != 0 ? "move" : "idle");

		_canWallJump = true;
		Sprite.Modulate = Colors.White;
		if (_timeSinceLastGroundedPosition >= TimeBetweenGroundedPositionTracking) {
			_lastGroundedPosition = GlobalPosition;
			_timeSinceLastGroundedPosition = 0f;
		}

		_remainingCoyoteTime = CoyoteTime;
		_velocity.Y = 0;

		if (_remainingInputBufferTime > 0 || Input.IsActionJustPressed("jump")) {
			_velocity.Y = JumpVelocity;
			_currentState = MovementState.Airborne;
		}
	}

	private void HandleClimbing(float delta) {
		if (_vineInRange == null) {
			_currentState = MovementState.Airborne;
			return;
		}

		if (!IsInstanceValid(_vineInRange)) {
			_currentState = MovementState.Airborne;
			return;
		}

		// TODO: Add climbing animation
		Sprite.Play("jump");
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
			_currentState = MovementState.Airborne;
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
			_velocity.X = Mathf.MoveToward(Velocity.X, 0,
				MaxSpeed / (IsOnFloor() ? DecelerationTime : AerialDecelerationTime) * delta);
		}


		return direction;
	}

	public override void _PhysicsProcess(double delta) {
		_remainingInputBufferTime -= (float)delta;

		_timeSinceLastGroundedPosition += (float)delta;

		_velocity = Velocity;

		RotationPoint.Rotation = RotationPoint.GlobalPosition.DirectionTo(GetGlobalMousePosition()).Angle();

		if (IsOnFloor()) {
			_currentState = MovementState.Grounded;
		}

		if (!ArcheryUsableWhenNotGrounded && !IsOnFloor()) {
			CancelArrow();
		}

		switch (_currentState) {
			case MovementState.Grounded:
				HandleGrounded((float)delta);
				break;
			case MovementState.Airborne:
				HandleAir((float)delta);
				break;
			case MovementState.WallGrab:
				HandleWallGrab((float)delta);
				break;
			case MovementState.WallSlide:
				HandleWallSlide((float)delta);
				break;
			case MovementState.Climbing:
				HandleClimbing((float)delta);
				break;
		}

		Velocity = _velocity;
		MoveAndSlide();

		if (Input.IsActionJustPressed("recall")) {
			Quiver?.Recall();
		}

		if (Input.IsActionJustPressed("interact")) {
			_interactableInRange?.Interact(this);
		}

		if (Input.IsActionJustPressed("next_arrow_type")) {
			Quiver?.ChangeArrowType(1);
		}

		if (ArcheryUsableWhenNotGrounded || _currentState == MovementState.Grounded) {
			if (Input.IsActionJustPressed("shoot")) {
				Arrow? arrow = Quiver?.GetArrow();
				if (arrow != null) {
					Bow?.ReadyArrow(arrow);
				}
			}

			if (Input.IsActionJustReleased("shoot")) {
				Bow?.ReleaseArrow();
			}
		}

		if (_currentState == MovementState.Grounded || _currentState == MovementState.Airborne) {
			GrabVine();
		}
	}

	private float _knownWallNormalX;

	private void HandleWallSlide(float delta) {
		float directionX = MoveHorizontal(delta);

		float wallNormal = GetWallNormal().X;
		bool isPerformingWallClimbInput =
			_velocity.Y >= 0 && IsOnWall() && Mathf.Sign(directionX) == -Mathf.Sign(wallNormal);

		if (isPerformingWallClimbInput) {
			_currentState = MovementState.WallGrab;
			return;
		}

		// Because there is no input IsOnWall() is likely false. Manually check if there is still a wall next to the player
		KinematicCollision2D? collision = MoveAndCollide(new Vector2(-_knownWallNormalX * 9f, 0), true);
		if (collision == null) {
			_currentState = MovementState.Airborne;
			return;
		}

		if (_remainingWallJumpGraceTime <= 0) {
			_currentState = MovementState.Airborne;
			return;
		}

		_velocity.Y = WallSlideSpeed;
		_velocity.X = 0f;

		bool holdingOppositeDirection = Mathf.Sign(directionX) == Mathf.Sign(wallNormal);
		_remainingWallJumpGraceTime =
			holdingOppositeDirection ? _remainingWallJumpGraceTime - delta : WallJumpGraceTime;

		if (_canWallJump && Input.IsActionJustPressed("jump")) {
			_velocity.Y = JumpVelocity;
			_velocity.X = Mathf.Sign(wallNormal) * MaxSpeed;
			_canWallJump = false;
			_currentState = MovementState.Airborne;
		}
	}

	public void HitKillZone() {
		GlobalPosition = _lastGroundedPosition;
	}
}
