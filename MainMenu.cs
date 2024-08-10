using Godot;

public partial class MainMenu : CanvasLayer {
	private void OnPlayButtonPressed() {
		GetTree().ChangeSceneToFile("res://main.tscn");
	}

	private void OnPlaygroundButtonPressed() {
		GetTree().ChangeSceneToFile("res://playground.tscn");
	}


	private void OnExitButtonPressed() {
		GetTree().Quit();
	}
}
