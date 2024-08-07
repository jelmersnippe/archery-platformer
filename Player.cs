using Godot;
using Godot.Collections;

public partial class Player : CharacterBody2D
{
	[Export] public PackedScene ArrowScene;
	
	[Export] public int InitialArrowCount = 5;

	private int _currentArrowCount;
	private Array<Node2D> _activeArrows = new();
	
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -600.0f;
	[Export] public float CoyoteTime = 0.1f;
	[Export] public float InputBufferTime = 0.1f;

	private float _remainingCoyoteTime = 0.1f;
	private float _remainingInputBufferTime = 0;

	[Export] public AnimatedSprite2D Sprite;

	[Export] public AnimationPlayer BowAnimationPlayer;
	[Export] public Node2D RotationPoint;
	[Export] public Node2D FiringPoint;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public override void _Ready()
	{
		_currentArrowCount = InitialArrowCount;
	}

	public override void _PhysicsProcess(double delta) {
		_remainingInputBufferTime -= (float)delta;
		
		RotationPoint.Rotation = RotationPoint.GlobalPosition.DirectionTo(GetGlobalMousePosition()).Angle();
		
		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			_remainingCoyoteTime -= (float)delta;
			CancelArrow();
			velocity.Y += _gravity * (float)delta;
			Sprite.Play("jump");
		}
		else
		{
			_remainingCoyoteTime = CoyoteTime;
		}

		if (_remainingInputBufferTime > 0 && IsOnFloor()) {
			velocity.Y = JumpVelocity;
		}
		if (Input.IsActionJustPressed("jump"))
		{
			if (IsOnFloor() || _remainingCoyoteTime > 0) {
				velocity.Y = JumpVelocity;
			}
			else {
				_remainingInputBufferTime = InputBufferTime;
			}
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		var direction = Input.GetAxis("move_left", "move_right");
		if (direction != 0)
		{
			velocity.X = direction * Speed;
			Sprite.FlipH = direction < 0;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		if (IsOnFloor())
		{
			Sprite.Play(direction != 0 ? "move" : "idle");
		}

		Velocity = velocity;
		MoveAndSlide();

		if (Input.IsActionJustPressed("shoot") && IsOnFloor())
		{
			ReadyArrow();
		}
		if (Input.IsActionJustReleased("shoot") && IsOnFloor())
		{
			ReleaseArrow();
		}
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
