using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class SeesawPlatform : StaticBody2D {
	[Export] public Area2D PlayerDetectionArea = null!;
	[Export] public CollisionShape2D CollisionShape2D = null!;
	[Export] public float RotationSpeed = 40f;

	private float _halfLength;
	private Player? _player;
		
	public override void _Ready()
	{
		PlayerDetectionArea.BodyEntered += PlayerDetectionAreaOnBodyEntered;
		PlayerDetectionArea.BodyExited += PlayerDetectionAreaOnBodyExited;
		_halfLength = CollisionShape2D.Shape.GetRect().Size.X / 2f;
	}

	private void PlayerDetectionAreaOnBodyExited(Node2D body) {
		if (body is Player player && player == _player) {
			_player = null;
		}
	}

	private void PlayerDetectionAreaOnBodyEntered(Node2D body) {
		if (body is Player player) {
			_player = player;
		}
	}

	public override void _Process(double delta)
	{
		if (_player == null || !IsInstanceValid(_player)) {
			RotationDegrees = Mathf.MoveToward(RotationDegrees, 0f, (RotationSpeed / 2f) * (float)delta);
			return;
		}

		var xDistance = _player.GlobalPosition.X - GlobalPosition.X;
		var percentageFromCenter = (Mathf.Abs(xDistance) / _halfLength);
		RotationDegrees += Mathf.Sign(xDistance) * (RotationSpeed * percentageFromCenter) * (float)delta;
	}
}
