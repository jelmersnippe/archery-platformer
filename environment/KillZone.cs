using Godot;
using System;

public partial class KillZone : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body) {
		if (body is not Player player) {
			return;
		}

		player.HitKillZone();
	}
}
