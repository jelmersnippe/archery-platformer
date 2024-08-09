using System;
using Godot;

public partial class SceneDoor : Area2D {
	[Export] public string TargetSceneName;
	[Export] public string TargetDoorId;
	[Export] public string Id;
	[Export] public Vector2I Direction;

	private bool _disabled;

	public override void _Ready() {
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		Direction = new Vector2I(Mathf.Sign(Direction.X), Mathf.Sign(Direction.Y));

		if (Direction.X != 0 && Direction.Y != 0) {
			throw new Exception($"Direction for SceneDoor {Id} is not one of the cardinal directions");
		}
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

		Vector2 directionToBody = GlobalPosition.DirectionTo(body.GlobalPosition);
		int xDirection = Mathf.Sign(directionToBody.X);
		int yDirection = Mathf.Sign(directionToBody.Y);
		if ((Direction.X != 0 && xDirection != Direction.X) ||
		    (Direction.Y != 0 && yDirection != Direction.Y)) {
			return;
		}

		SceneTransitionHandler.Instance.CallDeferred("HandleTransition", TargetSceneName, TargetDoorId);
	}
}
