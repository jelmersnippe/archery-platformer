using System.Runtime.InteropServices;
using Godot;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed = 50.0f;
	private readonly float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	[Export] public InputComponent InputComponent = null!;
	[Export] public HealthComponent HealthComponent = null!;
	[Export] public HurtboxComponent HurtboxComponent = null!;

	public override void _Ready() {
		HealthComponent.Died += QueueFree;
		HurtboxComponent.Hit += (component, direction) => {
			HealthComponent.TakeDamage(component.ContactDamage);
		};
	}

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
