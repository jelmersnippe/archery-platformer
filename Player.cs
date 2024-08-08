using Godot;
using Godot.Collections;

public partial class Player : CharacterBody2D
{
	[Export] public PackedScene ArrowScene;
	
	[Export] public int InitialArrowCount = 5;

	private int _currentArrowCount;
	private Array<Node2D> _activeArrows = new();
	
	[Export] public float Speed = 300.0f;

	[Export] public float MaxJumpHeight = 128f;
	[Export] public float JumpTimeToPeak = 0.5f;
	[Export] public float JumpTimeToGround = 0.3f;
	[Export] public float JumpDecelerationOnRelease = 0.5f;
	
	[Export] public float CoyoteTime = 0.1f;
	[Export] public float InputBufferTime = 0.1f;

	private float JumpVelocity => 2f * MaxJumpHeight / JumpTimeToPeak * -1f;
	private float JumpGravity => -2f * MaxJumpHeight / (JumpTimeToPeak * JumpTimeToPeak) * -1f;
	private float FallGravity => -2f * MaxJumpHeight / (JumpTimeToGround * JumpTimeToGround) * -1f;
	private float Gravity => Velocity.Y < 0f ? JumpGravity : FallGravity;

	private float _remainingCoyoteTime = 0.1f;
	private float _remainingInputBufferTime = 0;

	[Export] public AnimatedSprite2D Sprite;

	[Export] public AnimationPlayer BowAnimationPlayer;
	[Export] public Node2D RotationPoint;
	[Export] public Node2D FiringPoint;
	[Export] public Area2D GrabArea;

	private bool _isClimbing;

	public override void _Ready()
	{
		_currentArrowCount = InitialArrowCount;
		GrabArea.AreaEntered += GrabAreaOnAreaEntered;
		GrabArea.AreaExited += GrabAreaOnAreaExited;
	}

	private void GrabAreaOnAreaExited(Area2D area) {
		if (area is not Vine vine) {
			return;
		}

		if (vine == _vineInRange) {
			_vineInRange = null;
			_isClimbing = false;
		}
	}

	private Vine _vineInRange;

	private void GrabAreaOnAreaEntered(Area2D area) {
		if (area is not Vine vine) {
			return;
		}

		_vineInRange = vine;
	}

	private Vector2 _velocity;
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

		if (Input.IsActionJustPressed("jump"))
		{
			if ( _remainingCoyoteTime > 0) {
				_velocity.Y = JumpVelocity;
			}
			else {
				_remainingInputBufferTime = InputBufferTime;
			}
		}
		MoveHorizontal();
		GrabVine();
	}

	private void GrabVine() {
		if (!_isClimbing && _vineInRange != null && Input.IsActionPressed("move_up")) {
			_isClimbing = true;
			GlobalPosition = new Vector2(_vineInRange.GlobalPosition.X, GlobalPosition.Y);
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
		
		if (Input.IsActionJustPressed("jump"))
		{
			_velocity.Y = JumpVelocity;
		}
		var horizontalDirection = MoveHorizontal();
		
		Sprite.Play(horizontalDirection != 0 ? "move" : "idle");

		if (Input.IsActionJustPressed("shoot"))
		{
			ReadyArrow();
		}
		if (Input.IsActionJustReleased("shoot"))
		{
			ReleaseArrow();
		}
		
		GrabVine();
	}
	
	private void HandleClimbing(float delta) {
		if (!_isClimbing) {
			return;
		}

		_velocity = Vector2.Zero;
		_velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
		
		var direction = Input.GetAxis("move_up", "move_down");
		if (direction != 0)
		{
			_velocity.Y = direction * Speed;
		}
		else
		{
			_velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
		}
		
		if (Input.IsActionJustPressed("jump"))
		{
			_velocity.Y = JumpVelocity;
			_isClimbing = false;
		}
	}

	private float MoveHorizontal() {
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		var direction = Input.GetAxis("move_left", "move_right");
		if (direction != 0)
		{
			_velocity.X = direction * Speed;
			Sprite.FlipH = direction < 0;
		}
		else
		{
			_velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
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
		
		if (Input.IsActionJustPressed("recall"))
		{
			Recall();
		}
	}

	private Arrow _currentArrow;
	private void ReadyArrow()
	{
		if (_currentArrowCount <= 0 || _currentArrow != null)
		{
			return;
		}
		
		_currentArrow = ArrowScene.Instantiate<Arrow>();
		_currentArrow.Position = Vector2.Zero;
		FiringPoint.AddChild(_currentArrow);
		BowAnimationPlayer.Play("draw");
	}

	private void CancelArrow()
	{
		_currentArrow?.QueueFree();
		_currentArrow = null;
		BowAnimationPlayer.Stop();
	}

	private void ReleaseArrow()
	{
		if (_currentArrow == null)
		{
			return;
		}

		_currentArrowCount--;
		
		_currentArrow.Velocity = GlobalPosition.DirectionTo(GetGlobalMousePosition())* 1000f;

		_currentArrow.CollisionShape2D.SetDeferred("disabled", false);
		_currentArrow.Reparent(GetTree().CurrentScene);
		
		_activeArrows.Add(_currentArrow);
		_currentArrow.TransitionedToStuck += (arrow, stuckArrow) =>
		{
			_activeArrows.Remove(arrow);
			_activeArrows.Add(stuckArrow);
			stuckArrow.TreeExiting += () =>
			{
				_activeArrows.Remove(stuckArrow);
				_currentArrowCount++;
			};
		};
		_currentArrow = null;
		BowAnimationPlayer.Stop();
	}

	private void Recall()
	{
		foreach (var arrow in _activeArrows)
		{
			arrow.QueueFree();
			_currentArrowCount++;
		}

		_activeArrows.Clear();
	}
}
