using Godot;
using System;

public partial class Camera : Camera2D {
	private Player? _target;
	[Export] public Area2D Area2D;

	public override void _EnterTree() {
		SceneTransitionHandler.Instance.PlayerSpawned += InstanceOnPlayerSpawned;
	}

	private void InstanceOnPlayerSpawned(Player player) {
		_target = player;
	}

	public override void _Ready() {
		Area2D.AreaEntered += Area2DOnAreaEntered;
		Area2D.BodyExited += Area2DOnBodyExited;
	}

	private void Area2DOnBodyExited(Node2D body) {
		if (body is not Player) {
			return;
		}
		
		GD.Print("Resetting limits to follow player");
		ResetLimits();
	}

	private void Area2DOnAreaEntered(Area2D area) {
		if (area is not CameraZone cameraZone || cameraZone.CollisionShape2D.Shape is not RectangleShape2D rectShape) {
			return;
		}

		var areaCenter = cameraZone.CollisionShape2D.GlobalPosition;
		LimitLeft = Mathf.RoundToInt(areaCenter.X - rectShape.Size.X / 2f);
		LimitRight = Mathf.RoundToInt(areaCenter.X + rectShape.Size.X / 2f);
		LimitTop = Mathf.RoundToInt(areaCenter.Y - rectShape.Size.Y / 2f);
		LimitBottom = Mathf.RoundToInt(areaCenter.Y + rectShape.Size.Y / 2f);
	}

	private void ResetLimits() {
		LimitLeft = -100000000;
		LimitRight = 100000000;
		LimitTop = -100000000;
		LimitBottom = 100000000;
	}

	public override void _Process(double delta) {
		if (_target == null || !IsInstanceValid(_target)) {
			return;
		}

		GlobalPosition = _target.GlobalPosition;
	}
}
