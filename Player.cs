using Godot;
using Godot.Collections;

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
	[Export] public HealthComponent HealthComponent = null!;
	[Export] public HurtboxComponent HurtboxComponent = null!;
	[Export] public Knockable Knockable = null!;
	[Export] public PlayerInputComponent InputComponent = null!;

	[ExportCategory("Archery")] public Bow? Bow { get; private set; }
	public Quiver? Quiver { get; private set; }

	[Export] public Array<MovementState> CanUseArcheryStates = new() {
		MovementState.Grounded
	};

	[Export] public float DrawHorizontalSlowdown = 0.4f;
	[Export] public float DrawVerticalSlowdown = 0.6f;

	[ExportCategory("Movement")] [Export] public float AccelerationTime = 0.3f;
	[Export] public float DecelerationTime = 0.2f;
	[Export] public float MaxSpeed = 300.0f;
	[Export] public float AerialAccelerationTime = 0.6f;
	[Export] public float AerialDecelerationTime = 0.5f;
	[Export] public bool WallMovementAllowed;

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

	public enum MovementState {
		Grounded,
		Airborne,
		WallGrab,
		WallSlide,
		Climbing
	}

	private MovementState _currentState = MovementState.Grounded;

	[Export] public float KillZoneControllLossTime = 0.5f;
	public Vector2 LastSafePoint;

	public override void _Ready() {
		GrabArea.AreaEntered += GrabAreaOnAreaEntered;
		GrabArea.AreaExited += GrabAreaOnAreaExited;

		InteractableArea.AreaEntered += InteractableAreaOnAreaEntered;
		InteractableArea.AreaExited += InteractableAreaOnAreaExited;

		HurtboxComponent.Hit += HurtboxComponentOnHit;
		HealthComponent.Died += HealthComponentOnDied;
	}

	private void HealthComponentOnDied() {
		SceneTreeTimer? timer = GetTree().CreateTimer(1.5f);
		timer.Timeout += () => GetTree().ReloadCurrentScene();
	}

	private void HurtboxComponentOnHit(HitboxComponent hitboxComponent, Vector2 direction) {
		HealthComponent.TakeDamage(hitboxComponent.ContactDamage);
		Knockable.ApplyKnockback(hitboxComponent.KnockbackForce * -direction);
		InputComponent.SetDisabled(true, Knockable.ControlLossTime);
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
			_currentState = MovementState.Airborne;
		}
	}

	public void EquipBow(Bow? bow) {
		if (bow == null) {
			Bow?.QueueFree();
		}
		else {
			BowOffset.AddChild(bow);
			bow.RotationPoint = RotationPoint;
		}

		Bow = bow;
		EmitSignal(SignalName.BowEquipped, bow);
	}

	public void EquipQuiver(Quiver? quiver) {
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

		if (_velocity.Y < 0 && InputComponent.IsActionJustReleased("jump")) {
			_velocity.Y *= JumpDecelerationOnRelease;
		}


		if (InputComponent.IsActionJustPressed("jump")) {
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
		if (!WallMovementAllowed) {
			_currentState = MovementState.Airborne;
			return;
		}

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

		if (_canWallJump && InputComponent.IsActionJustPressed("jump")) {
			_velocity.Y = JumpVelocity;
			_velocity.X = Mathf.Sign(wallNormal) * MaxSpeed;
			_canWallJump = false;
			Sprite.Modulate = Colors.Red;
			_currentState = MovementState.Airborne;
		}
	}

	private void GrabVine() {
		if (_vineInRange != null && InputComponent.GetDirectionalInput().Y < 0) {
			if (IsOnFloor()) {
				// Move slightly off ground
				Position += new Vector2(0, -4);
			}

			_currentState = MovementState.Climbing;
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

		_remainingCoyoteTime = CoyoteTime;
		_velocity.Y = 0;

		if ((_remainingInputBufferTime > 0 && !InputComponent.Disabled) || InputComponent.IsActionJustPressed("jump")) {
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

		float verticalDirection = InputComponent.GetDirectionalInput().Y;

		if (verticalDirection != 0) {
			_velocity.Y = Mathf.MoveToward(Velocity.Y, ClimbingMaxSpeed * verticalDirection,
				ClimbingMaxSpeed / ClimbingAccelerationTime * delta);
		}
		else {
			_velocity.Y = Mathf.MoveToward(Velocity.Y, 0, ClimbingMaxSpeed / ClimbingDecelerationTime * delta);
		}

		if (InputComponent.IsActionJustPressed("jump")) {
			_velocity.Y = JumpVelocity;
			_velocity.X = MoveHorizontal(delta) * MaxSpeed;
			_currentState = MovementState.Airborne;
		}
	}

	private float MoveHorizontal(float delta) {
		if (InputComponent.Disabled) {
			return 0f;
		}

		float direction = InputComponent.GetDirectionalInput().X;

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

		_velocity = Velocity;

		RotationPoint.Rotation = RotationPoint.GlobalPosition.DirectionTo(GetGlobalMousePosition()).Angle();

		if (IsOnFloor()) {
			_currentState = MovementState.Grounded;
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

		if (InputComponent.IsActionJustPressed("recall") && Quiver is RecallQuiver recallQuiver) {
			recallQuiver.Recall();
		}

		if (InputComponent.IsActionJustPressed("interact")) {
			_interactableInRange?.Interact(this);
		}

		if (InputComponent.IsActionJustPressed("next_arrow_type")) {
			Quiver?.ChangeArrowType(1);
		}

		if (!CanUseArcheryStates.Contains(_currentState)) {
			CancelArrow();
		}
		else {
			if (InputComponent.IsActionJustPressed("shoot")) {
				Arrow? arrow = Quiver?.GetArrow();
				if (arrow != null) {
					Bow?.ReadyArrow(arrow);
				}
			}

			if (InputComponent.IsActionJustReleased("shoot")) {
				Bow?.ReleaseArrow();
			}
		}

		if (_currentState == MovementState.Grounded || _currentState == MovementState.Airborne) {
			GrabVine();
		}
	}

	private float _knownWallNormalX;

	private void HandleWallSlide(float delta) {
		if (!WallMovementAllowed) {
			_currentState = MovementState.Airborne;
			return;
		}

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

		if (_canWallJump && InputComponent.IsActionJustPressed("jump")) {
			_velocity.Y = JumpVelocity;
			_velocity.X = Mathf.Sign(wallNormal) * MaxSpeed;
			_canWallJump = false;
			Sprite.Modulate = Colors.Red;
			_currentState = MovementState.Airborne;
		}
	}

	public void HitKillZone() {
		GlobalPosition = LastSafePoint;
		Velocity = Vector2.Zero;
		InputComponent.SetDisabled(true, KillZoneControllLossTime);
	}
}
