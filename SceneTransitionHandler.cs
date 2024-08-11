using System;
using System.Linq;
using Godot;

public partial class SceneTransitionHandler : Node {
	[Signal]
	public delegate void PlayerSpawnedEventHandler(Player player);

	[Signal]
	public delegate void TileMapSpawnedEventHandler(TileMap tileMap);

	private readonly PackedScene _playerScene =
		ResourceLoader.Load<PackedScene>("res://player.tscn");


	public Player? Player { get; private set; }

	private static SceneTransitionHandler? _instance;
	[Export] public Label Title = null!;
	public static SceneTransitionHandler Instance => _instance!;

	private bool _isTransitioning;
	private string? _targetDoorId = "spawn";
	private SceneDoor? _targetDoor;
	private Vector2 _playerRelativePositionToDoor = Vector2.Zero;

	public override void _EnterTree() {
		if (_instance != null) {
			QueueFree();
			return;
		}

		_instance = this;

		GetTree().NodeAdded += OnNodeAdded;
	}

	private void OnNodeAdded(Node node) {
		if (node is SceneDoor door && (_targetDoorId == null || _targetDoorId == door.Id)) {
			MovePlayerToDoor(door);
		}

		if (node is TileMap tileMap) {
			EmitSignal(SignalName.TileMapSpawned, tileMap);
		}
	}

	private SceneDoor? GetTargetDoor(string targetDoorId) {
		return GetTree().CurrentScene.GetChildren().OfType<SceneDoor>().FirstOrDefault(x => x.Id == targetDoorId);
	}

	private Player MovePlayerToDoor(SceneDoor door) {
		_targetDoor = door;
		if (Player == null || !IsInstanceValid(Player)) {
			Player = _playerScene.Instantiate<Player>();
			GetTree().CurrentScene.CallDeferred("add_child", Player);
		}
		else {
			Player.CallDeferred("reparent", GetTree().CurrentScene);
		}

		Vector2 doorOffset = door.Direction.X == 0
			// Vertical transition, keep horizontal position
			? new Vector2(_playerRelativePositionToDoor.X, 0)
			// Horizontal transition, keep vertical position
			: new Vector2(0, _playerRelativePositionToDoor.Y);

		door.BodyExited += DoorOnBodyExited;

		Player.GlobalPosition = door.GlobalPosition + doorOffset;
		EmitSignal(SignalName.PlayerSpawned, Player);

		return Player;
	}

	private void DoorOnBodyExited(Node2D body) {
		if (body is Player) {
			_isTransitioning = false;
		}

		if (_targetDoor != null) {
			_targetDoor.BodyExited -= DoorOnBodyExited;
		}
	}

	public void HandleTransition(SceneDoor fromDoor) {
		if (_isTransitioning) {
			return;
		}

		if (string.IsNullOrWhiteSpace(fromDoor.TargetDoorId) ||
			string.IsNullOrWhiteSpace(fromDoor.TargetSceneName)) {
			return;
		}

		if (GetTree().CurrentScene == null) {
			return;
		}

		_isTransitioning = true;

		_targetDoorId = fromDoor.TargetDoorId;
		_playerRelativePositionToDoor = Player!.GlobalPosition - fromDoor.GlobalPosition;

		if (string.Equals(fromDoor.TargetSceneName, GetTree().CurrentScene.Name, StringComparison.OrdinalIgnoreCase)) {
			SceneDoor? targetDoor = GetTargetDoor(fromDoor.TargetDoorId);
			if (targetDoor == null) {
				return;
			}

			MovePlayerToDoor(targetDoor);
			return;
		}

		Player?.Reparent(GetTree().Root);
		Player?.Quiver?.Restock();

		GetTree().ChangeSceneToFile($"res://{fromDoor.TargetSceneName}.tscn");
	}
}
