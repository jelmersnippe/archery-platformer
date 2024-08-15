using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed = 50.0f;
	private readonly float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	[Export] public InputComponent InputComponent = null!;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor()) {
			velocity.Y += _gravity * (float)delta;
		}
		
		Vector2 direction = InputComponent.GetDirection();
		if (direction != Vector2.Zero && IsOnFloor())
		{
			velocity.X = Mathf.MoveToward(Velocity.X, direction.X * Speed, Speed);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
