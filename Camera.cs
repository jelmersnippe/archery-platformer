using Godot;

public partial class Camera : Camera2D {
	private Player? _target;
	[Export] public Area2D Area2D;

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

		var offset = new Vector2(tileMap.CellQuadrantSize / 2f, tileMap.CellQuadrantSize / 2f);
		Vector2 areaCenter = tileMapRect.GetCenter() * tileMap.CellQuadrantSize + offset;
		Vector2I worldTileMapSize = tileMapRect.Size * tileMap.CellQuadrantSize;
		Vector2 distanceToTileMapSide = new Vector2(worldTileMapSize.X, worldTileMapSize.Y) / 2f;
		LimitLeft = Mathf.RoundToInt(areaCenter.X - distanceToTileMapSide.X);
		LimitRight = Mathf.RoundToInt(areaCenter.X + distanceToTileMapSide.X);
		LimitTop = Mathf.RoundToInt(areaCenter.Y - distanceToTileMapSide.Y);
		LimitBottom = Mathf.RoundToInt(areaCenter.Y + distanceToTileMapSide.Y);
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

		GlobalPosition = _target.GlobalPosition;
	}
}
