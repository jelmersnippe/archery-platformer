using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class Knockable : Node {
	[Export] public bool LockX;
	[Export] public bool LockY;
	private CharacterBody2D _body = null!;

	public override void _Ready()
	{
		_body = GetParent<CharacterBody2D>();
	}

	public void ApplyKnockback(Vector2 force) {
		Vector2 adjustedForce = force;
		if (LockX) {
			adjustedForce.X = 0f;
		}
		if (LockY) 
		{
			adjustedForce.Y = 0f;
		}
		
		_body.Velocity = adjustedForce;
	}
}
