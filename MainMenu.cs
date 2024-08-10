using Godot;

public partial class MainMenu : CanvasLayer {
	public override void _Ready() {
		GlobalTriggerState.Reset();
	}

	private void OnPlayButtonPressed() {
		GetTree().ChangeSceneToFile("res://forest/forest2.tscn");
	}

	private void OnPlaygroundButtonPressed() {
		GetTree().ChangeSceneToFile("res://playground.tscn");
	}

	private void OnExitButtonPressed() {
		GetTree().Quit();
	}
}
