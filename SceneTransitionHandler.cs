using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class SceneTransitionHandler : Node {
	[Signal]
	public delegate void PlayerSpawnedEventHandler(Player player);
	
	private readonly PackedScene _playerScene =
		ResourceLoader.Load<PackedScene>("res://player.tscn");

	private Player? _player;
	
	private static SceneTransitionHandler? _instance;
	[Export] public Label Title = null!;
	public static SceneTransitionHandler Instance => _instance!;

	private bool _playerSpawned;
	private string? _targetDoorId;

	public override void _EnterTree() {
		if (_instance != null) {
			QueueFree();
			return;
		}

		_instance = this;

		GetTree().NodeAdded += OnNodeAdded;
	}

	private void OnNodeAdded(Node node) {
		if (_playerSpawned) {
			return;
		}
		
		if (node is SceneDoor door && (_targetDoorId == null || _targetDoorId == door.Id)) {
			SpawnPlayerAtDoor(door);
		}
	}

	private SceneDoor? GetTargetDoor(string targetDoorId) {
		return GetTree().CurrentScene.GetChildren().OfType<SceneDoor>().FirstOrDefault(x => x.Id == targetDoorId);
	}
	
	private Player MovePlayerToDoor(SceneDoor door) {
		door.Disable();
		if (_player == null || !IsInstanceValid(_player)) {
			_player = _playerScene.Instantiate<Player>();
			EmitSignal(SignalName.PlayerSpawned, _player);
		}
		_player.GlobalPosition = door.GlobalPosition + door.Direction * 20;
		return _player;
	}

	private void SpawnPlayerAtDoor(SceneDoor door) {
		var player =MovePlayerToDoor(door);
		GetTree().CurrentScene.CallDeferred("add_child", player);
	}
	 
	public void HandleTransition(string targetScene, string targetDoorId) {
		_playerSpawned = false;
		_targetDoorId = targetDoorId;
		if (string.Equals(targetScene, GetTree().CurrentScene.Name, StringComparison.OrdinalIgnoreCase)) {
			var targetDoor = GetTargetDoor(targetDoorId);
			if (targetDoor == null) {
				return;
			}
			MovePlayerToDoor(targetDoor);
			return;
		}
		
		GetTree().ChangeSceneToFile($"res://{targetScene}.tscn");
	}
}
