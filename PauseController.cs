using System;
using Godot;

public partial class PauseController : Node {
	[Signal]
	public delegate void PauseStateChangedEventHandler(bool paused);

	private static PauseController? _instance;

	private bool _buttonHeld;
	public static PauseController Instance => _instance!;

	public override void _EnterTree() {
		if (_instance != null) {
			QueueFree();
			return;
		}

		_instance = this;
	}

	public override void _Ready() {
		ProcessMode = ProcessModeEnum.Always;
	}

	public override void _Input(InputEvent @event) {
		if (string.Equals(GetTree().CurrentScene.Name, "MainMenu", StringComparison.OrdinalIgnoreCase)) {
			return;
		}

		if (@event.IsActionPressed("pause") && !_buttonHeld) {
			Toggle();
			_buttonHeld = true;
		}

		if (@event.IsActionReleased("pause")) {
			_buttonHeld = false;
		}
	}

	public void Toggle() {
		GetTree().Paused = !GetTree().Paused;
		Engine.TimeScale = GetTree().Paused ? 0 : 1;

		EmitSignal(SignalName.PauseStateChanged, GetTree().Paused);
	}
}
