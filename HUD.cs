using Godot;

public partial class HUD : CanvasLayer {
	[Export] public Label ArrowCount = null!;

	private Quiver? _currentQuiver;

	public override void _Ready() {
		ArrowCount.Hide();
		SceneTransitionHandler.Instance.PlayerSpawned += InstanceOnPlayerSpawned;
		if (SceneTransitionHandler.Instance.Player != null) {
			SceneTransitionHandler.Instance.Player.QuiverEquipped += SetQuiver;
		}
	}

	private void InstanceOnPlayerSpawned(Player player) {
		player.QuiverEquipped += SetQuiver;
		SetQuiver(player.Quiver);
	}

	private void SetQuiver(Quiver? quiver) {
		if (quiver == _currentQuiver) {
			return;
		}

		if (quiver == null) {
			ArrowCount.Hide();
			if (_currentQuiver != null) {
				_currentQuiver.ArrowCountChanged -= QuiverOnArrowCountChanged;
			}

			_currentQuiver = null;
			return;
		}

		ArrowCount.Show();

		quiver.ArrowCountChanged += QuiverOnArrowCountChanged;
		_currentQuiver = quiver;
	}

	private void QuiverOnArrowCountChanged(int current, int max) {
		ArrowCount.Text = $"Arrows: {current}/{max}";
	}
}
