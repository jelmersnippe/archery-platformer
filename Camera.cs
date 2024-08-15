using Godot;

public partial class Camera : Camera2D {
	private Player? _target;
	[Export] public Area2D Area2D = null!;
	[Export] public float SmoothingX = 0.06f;
	[Export] public float SmoothingY = 0.02f;
	[Export] public float LookAheadX = 48f;
	[Export] public float FallingLookAheadY = 96f;
	[Export] public float DirectionFlipDeadZone = 128f;

	private int _lookAheadDirection;

	public override void _EnterTree() {
		SceneTransitionHandler.Instance.PlayerSpawned += InstanceOnPlayerSpawned;
		SceneTransitionHandler.Instance.TileMapSpawned += SetLimitsToTileMap;
	}

	private void InstanceOnPlayerSpawned(Player player) {
		_target = player;
	}

	public override void _Ready() {
		Area2D.AreaEntered += Area2DOnAreaEntered;
	}

	private void SetLimitsToTileMap(TileMap tileMap) {
		Rect2I tileMapRect = tileMap.GetUsedRect();

		Vector2I topLeft = tileMapRect.Position * tileMap.CellQuadrantSize;
		Vector2I bottomRight = tileMapRect.End * tileMap.CellQuadrantSize;

		LimitLeft = topLeft.X;
		LimitRight = bottomRight.X;
		LimitTop = topLeft.Y;
		LimitBottom = bottomRight.Y;
	}

	private void Area2DOnAreaEntered(Area2D area) {
		if (area is not CameraZone cameraZone || cameraZone.CollisionShape2D.Shape is not RectangleShape2D rectShape) {
			return;
		}

		Vector2 areaCenter = cameraZone.CollisionShape2D.GlobalPosition;
		LimitLeft = Mathf.RoundToInt(areaCenter.X - rectShape.Size.X / 2f);
		LimitRight = Mathf.RoundToInt(areaCenter.X + rectShape.Size.X / 2f);
		LimitTop = Mathf.RoundToInt(areaCenter.Y - rectShape.Size.Y / 2f);
		LimitBottom = Mathf.RoundToInt(areaCenter.Y + rectShape.Size.Y / 2f);
	}

	public override void _Process(double delta) {
		if (_target == null || !IsInstanceValid(_target)) {
			return;
		}

		var targetDirection = Mathf.Sign(_target.Velocity.X);
		if (targetDirection != 0 && targetDirection != _lookAheadDirection) {
			if (GlobalPosition.DistanceTo(_target.GlobalPosition) < DirectionFlipDeadZone) {
				return;
			}
			_lookAheadDirection = targetDirection;
		}

		var targetPosition = _target.GlobalPosition +  new Vector2(_lookAheadDirection * LookAheadX, _target.Velocity.Y > 0 ? FallingLookAheadY : 0f);
		GlobalPosition = new Vector2() {
			X = Mathf.Lerp(GlobalPosition.X, targetPosition.X, SmoothingX),
			Y = Mathf.Lerp(GlobalPosition.Y, targetPosition.Y, SmoothingY),
		};
	}
}
