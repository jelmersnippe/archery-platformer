using Godot;
using System;
using Godot.Collections;

public partial class Player : CharacterBody2D
{
	[Export] public PackedScene ArrowScene;
	
	[Export] public int InitialArrowCount = 5;

	private int _currentArrowCount;
	private Array<Node2D> _activeArrows = new();
	
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -600.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public override void _Ready()
	{
		_currentArrowCount = InitialArrowCount;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y += gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		var direction = Input.GetAxis("move_left", "move_right");
		if (direction != 0)
		{
			velocity.X = direction * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();

		if (Input.IsActionJustPressed("shoot") && IsOnFloor())
		{
			Shoot();
		}
		if (Input.IsActionJustPressed("recall"))
		{
			Recall();
		}
	}

	private void Shoot()
	{
		if (_currentArrowCount <= 0)
		{
			return;
		}

		_currentArrowCount--;
		
		var arrow = ArrowScene.Instantiate<Arrow>();
		var direction = GlobalPosition.DirectionTo(GetGlobalMousePosition());
		arrow.Velocity = direction * 1000f;
		arrow.GlobalPosition = GlobalPosition;
		arrow.Rotation = direction.Angle();

		GetTree().CurrentScene.CallDeferred("add_child", arrow);
		
		_activeArrows.Add(arrow);
		arrow.TransitionedToStuck += stuckArrow =>
		{
			_activeArrows.Remove(arrow);
			_activeArrows.Add(stuckArrow);
			stuckArrow.LifeTimeRanOut += () =>
			{
				_activeArrows.Remove(stuckArrow);
				_currentArrowCount++;
			};
		};
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
