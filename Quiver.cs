using Godot;
using Godot.Collections;

public partial class Quiver : Node {
	[Signal]
	public delegate void ArrowCountChangedEventHandler(int current, int max);

	[Export] public PackedScene ArrowScene = null!;
	[Export] public int InitialArrowCount = 5;

	private int _maxArrowCount;
	private int _availableArrowCount;
	private Array<Node2D> _activeArrows = new();

	public override void _Ready() {
		_maxArrowCount = InitialArrowCount;
		_availableArrowCount = _maxArrowCount;
		NotifyArrowChanges();
	}

	public void NotifyArrowChanges() {
		EmitSignal(SignalName.ArrowCountChanged, _availableArrowCount, _maxArrowCount);
	}

	public void Recall() {
		foreach (Node2D? arrow in _activeArrows) {
			arrow.QueueFree();
			_availableArrowCount = Mathf.Clamp(_availableArrowCount + 1, 0, _maxArrowCount);
		}

		_activeArrows.Clear();
		NotifyArrowChanges();
	}

	public Arrow? GetArrow() {
		if (_availableArrowCount <= 0) {
			return null;
		}

		_availableArrowCount = Mathf.Clamp(_availableArrowCount - 1, 0, _maxArrowCount);
		NotifyArrowChanges();
		var arrow = ArrowScene.Instantiate<Arrow>();

		// TODO: Move into specific RecallQuiver -> normal quiver does not automatically get arrows back
		// arrow.Released += CurrentArrowOnReleased;

		return arrow;
	}

	public void ReturnArrow(Arrow arrow) {
		_activeArrows.Remove(arrow);
		_availableArrowCount = Mathf.Clamp(_availableArrowCount + 1, 0, _maxArrowCount);
		NotifyArrowChanges();
	}

	// TODO: Move into specific RecallQuiver -> normal quiver does not automatically get arrows back
	// private void CurrentArrowOnReleased(Arrow arrow) {
	// 	_activeArrows.Add(arrow);
	// 	
	// 	arrow.Released -= CurrentArrowOnReleased;

	// 	arrow.TransitionedToStuck += (shotArrow, stuckArrow) => {
	// 		_activeArrows.Remove(shotArrow);
	// 		_activeArrows.Add(stuckArrow);
	// 		stuckArrow.TreeExiting += () => {
	// 			_activeArrows.Remove(stuckArrow);
	// 			_availableArrowCount = Mathf.Clamp(_availableArrowCount + 1, 0, _maxArrowCount);
	// 			NotifyArrowChanges();
	// 		};
	// 	};
	// }

	public void Restock() {
		_availableArrowCount = _maxArrowCount;
		NotifyArrowChanges();
	}
}
