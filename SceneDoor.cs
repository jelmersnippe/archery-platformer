using Godot;
using System;

public partial class SceneDoor : Area2D {
	[Export] public string TargetSceneName;
	[Export] public string TargetDoorId;
	[Export] public string Id;
	[Export] public Vector2 Direction;

	private bool _disabled = false;

	public override void _Ready() {
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	private void OnBodyExited(Node2D body) {
		_disabled = false;
	}

	public void Disable() {
		_disabled = true;
	}

	private void OnBodyEntered(Node2D body) {
		if (_disabled || body is not Player) {
			return;
		}

		SceneTransitionHandler.Instance.CallDeferred("HandleTransition", TargetSceneName, TargetDoorId);
	}
}
