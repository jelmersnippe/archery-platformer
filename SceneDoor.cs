using System;
using Godot;

public partial class SceneDoor : Area2D {
	[Export] public string? TargetSceneName;
	[Export] public string? TargetDoorId;
	[Export] public string Id = string.Empty;
	[Export] public Vector2I Direction;

	public override void _Ready() {
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		Direction = new Vector2I(Mathf.Sign(Direction.X), Mathf.Sign(Direction.Y));

		if (Direction.X != 0 && Direction.Y != 0) {
			throw new Exception($"Direction for SceneDoor {Id} is not one of the cardinal directions");
		}
	}

	private void OnBodyExited(Node2D body) {
		if (string.IsNullOrWhiteSpace(TargetSceneName) || string.IsNullOrWhiteSpace(TargetDoorId)) {
			return;
		}

		if (body is not Player) {
			return;
		}

		var directionToPlayer = GlobalPosition.DirectionTo(body.GlobalPosition);
		if (Direction.X != 0) {
			if (Mathf.Sign(directionToPlayer.X) != Direction.X) {
				SceneTransitionHandler.Instance.CallDeferred("HandleTransition", this);
			}
		}
		else {
			if (Mathf.Sign(directionToPlayer.Y) != Direction.Y) {
				SceneTransitionHandler.Instance.CallDeferred("HandleTransition", this);
			}
		}
	}

	private void OnBodyEntered(Node2D body) {
		if (string.IsNullOrWhiteSpace(TargetSceneName) || string.IsNullOrWhiteSpace(TargetDoorId)) {
			return;
		}

		if (body is not Player) {
			return;
		}

		Vector2 directionToBody = GlobalPosition.DirectionTo(body.GlobalPosition);
		int xDirection = Mathf.Sign(directionToBody.X);
		int yDirection = Mathf.Sign(directionToBody.Y);
		if ((Direction.X != 0 && xDirection != Direction.X) ||
			(Direction.Y != 0 && yDirection != Direction.Y)) {
			return;
		}


		SceneTransitionHandler.Instance.CallDeferred("HandleTransition", this);
	}
}
