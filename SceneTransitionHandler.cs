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

	private string? _targetDoorId = "main-1";
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
		if (Player == null || !IsInstanceValid(Player)) {
			Player = _playerScene.Instantiate<Player>();
			GetTree().CurrentScene.CallDeferred("add_child", Player);
		}
		else {
			Player.Reparent(GetTree().CurrentScene);
		}

		Vector2 invertedDoorOffset = door.Direction.X != 0
			? new Vector2(-_playerRelativePositionToDoor.X, _playerRelativePositionToDoor.Y)
			: new Vector2(_playerRelativePositionToDoor.X, -_playerRelativePositionToDoor.Y);
		// TODO: Use player collisionshape size
		Player.GlobalPosition = door.GlobalPosition + door.Direction * 16 + invertedDoorOffset;
		EmitSignal(SignalName.PlayerSpawned, Player);
		return Player;
	}

	public void HandleTransition(SceneDoor fromDoor) {
		if (string.IsNullOrWhiteSpace(fromDoor.TargetDoorId) || string.IsNullOrWhiteSpace(fromDoor.TargetSceneName)) {
			return;
		}

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

		Player.Reparent(GetTree().Root);
		GetTree().ChangeSceneToFile($"res://{fromDoor.TargetSceneName}.tscn");
	}
}
