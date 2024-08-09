using Godot;
using System;

public partial class Camera : Camera2D {
	private Node2D? _target;

	public override void _EnterTree() {
		SceneTransitionHandler.Instance.PlayerSpawned += player => _target = player;
	}

	public override void _Process(double delta)
	{
		if (_target == null || !IsInstanceValid(_target)) {
			return;
		}

		GlobalPosition = _target.GlobalPosition;
	}
}
