using Godot;
using Godot.Collections;

public partial class Quiver : Node {
	[Signal]
	public delegate void ArrowCountChangedEventHandler(int current, int max);

	[Export] public PackedScene ArrowScene = null!;
	[Export] public int InitialArrowCount = 5;

	private int _maxArrowCount;
	private Array<Node2D> _activeArrows = new();
	private Arrow? _currentArrow;

	public override void _Ready() {
		_maxArrowCount = InitialArrowCount;
		NotifyArrowChanges();
	}

	public void NotifyArrowChanges() {
		EmitSignal(SignalName.ArrowCountChanged, _maxArrowCount - _activeArrows.Count, _maxArrowCount);
	}

	public void Recall() {
		foreach (Node2D? arrow in _activeArrows) {
			arrow.QueueFree();
		}

		_activeArrows.Clear();
		NotifyArrowChanges();
	}

	public Arrow? GetArrow() {
		if (_activeArrows.Count >= _maxArrowCount || _currentArrow != null) {
			return null;
		}

		_currentArrow = ArrowScene.Instantiate<Arrow>();
		_currentArrow.Released += CurrentArrowOnReleased;
		return _currentArrow;
	}

	private void CurrentArrowOnReleased(Arrow activeArrow) {
		if (activeArrow != _currentArrow) {
			return;
		}

		_activeArrows.Add(activeArrow);
		NotifyArrowChanges();
		_currentArrow.Released -= CurrentArrowOnReleased;

		_currentArrow.TransitionedToStuck += (arrow, stuckArrow) => {
			_activeArrows.Remove(arrow);
			_activeArrows.Add(stuckArrow);
			stuckArrow.TreeExiting += () => {
				_activeArrows.Remove(stuckArrow);
				NotifyArrowChanges();
			};
		};

		_currentArrow = null;
	}
}
