using Godot;

public partial class HUD : CanvasLayer {
	[Export] public Label ArrowCountDisplay = null!;
	[Export] public TextureRect ArrowTypeDisplay = null!;

	private Quiver? _currentQuiver;

	public override void _Ready() {
		ArrowCountDisplay.Hide();
		ArrowTypeDisplay.Hide();
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
			ArrowCountDisplay.Hide();
			ArrowTypeDisplay.Hide();
			if (_currentQuiver != null) {
				_currentQuiver.ArrowCountChanged -= QuiverOnArrowCountChanged;
				_currentQuiver.ArrowTypeChanged -= QuiverOnArrowTypeChanged;
			}

			_currentQuiver = null;
			return;
		}

		ArrowCountDisplay.Show();
		ArrowTypeDisplay.Show();

		quiver.ArrowCountChanged += QuiverOnArrowCountChanged;
		quiver.ArrowTypeChanged += QuiverOnArrowTypeChanged;
		_currentQuiver = quiver;
	}

	private void QuiverOnArrowTypeChanged(ArrowType? arrowType) {
		if (arrowType == null) {
			ArrowTypeDisplay.Hide();
			return;
		}

		ArrowTypeDisplay.Show();
		ArrowTypeDisplay.Texture = arrowType.DisplaySprite;
	}

	private void QuiverOnArrowCountChanged(int current, int max) {
		ArrowCountDisplay.Text = $"Arrows: {current}/{max}";
	}
}
