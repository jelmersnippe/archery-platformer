using Godot;

public partial class PauseMenu : CanvasLayer {
	public override void _EnterTree() {
		PauseController.Instance.PauseStateChanged += InstanceOnPauseStateChanged;
	}

	public override void _ExitTree() {
		PauseController.Instance.PauseStateChanged -= InstanceOnPauseStateChanged;
	}

	public override void _Ready() {
		ProcessMode = ProcessModeEnum.Always;
		Hide();
	}

	private void InstanceOnPauseStateChanged(bool paused) {
		if (paused) {
			Show();
		}
		else {
			Hide();
		}
	}

	private void OnResumeButtonPressed() {
		PauseController.Instance.Toggle();
	}

	private void OnRestartButtonPressed() {
		GlobalTriggerState.Reset();
		GetTree().ReloadCurrentScene();
		PauseController.Instance.Toggle();
	}

	private void OnMainMenuButtonPressed() {
		GetTree().ChangeSceneToFile("res://menus/main_menu.tscn");
		PauseController.Instance.Toggle();
	}

	private void OnExitButtonPressed() {
		GetTree().Quit();
	}
}
