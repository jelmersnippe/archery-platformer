using System.Linq;
using Godot;
using Godot.Collections;

public partial class Quiver : Node {
	[Signal]
	public delegate void ArrowCountChangedEventHandler(int current, int max);

	[Signal]
	public delegate void ArrowTypeChangedEventHandler(ArrowType? arrowType);

	[Export] public int InitialArrowCount = 5;

	[Export] public Array<ArrowTypeUpgrade> UnlockableArrows = new();
	[Export] public Array<ArrowType> ArrowTypes = new();
	private ArrowType? _currentArrowType;

	private int _maxArrowCount;
	private int _availableArrowCount;
	private Array<Node2D> _activeArrows = new();

	public override void _Ready() {
		_maxArrowCount = InitialArrowCount;
		_availableArrowCount = _maxArrowCount;

		SetArrowType(0);

		NotifyArrowChanges();

		foreach (ArrowTypeUpgrade unlockableArrow in UnlockableArrows) {
			if (GlobalTriggerState.GetTriggerState(unlockableArrow.Trigger)) {
				AddArrowType(unlockableArrow.ArrowType);
			}
			else {
				RemoveArrowType(unlockableArrow.ArrowType);
			}
		}

		GlobalTriggerState.TriggerChanged += TriggerChanged;
	}

	private void TriggerChanged(Trigger trigger, bool state) {
		ArrowTypeUpgrade? unlockableArrow = UnlockableArrows.FirstOrDefault(x => x.Trigger == trigger);
		if (unlockableArrow == null) {
			return;
		}

		if (state) {
			AddArrowType(unlockableArrow.ArrowType);
		}
		else {
			RemoveArrowType(unlockableArrow.ArrowType);
		}
	}

	private void AddArrowType(ArrowType arrowType) {
		if (ArrowTypes.Contains(arrowType)) {
			return;
		}

		ArrowTypes.Add(arrowType);
	}

	private void RemoveArrowType(ArrowType arrowType) {
		ArrowTypes.Remove(arrowType);
	}

	public void NotifyArrowChanges() {
		EmitSignal(SignalName.ArrowCountChanged, _availableArrowCount, _maxArrowCount);
	}

	public void NotifyArrowTypeChanged() {
		EmitSignal(SignalName.ArrowTypeChanged, _currentArrowType!);
	}

	public void ChangeArrowType(int change) {
		if (_currentArrowType == null) {
			SetArrowType(0);
			return;
		}

		int currentIndex = ArrowTypes.IndexOf(_currentArrowType);
		if (currentIndex == -1) {
			_currentArrowType = null;
			SetArrowType(0);
			return;
		}

		SetArrowType((currentIndex + change) % ArrowTypes.Count);
	}

	private void SetArrowType(int index) {
		if (index < 0 || index >= ArrowTypes.Count) {
			return;
		}

		_currentArrowType = ArrowTypes[index];
		NotifyArrowTypeChanged();
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
		if (_availableArrowCount <= 0 || _currentArrowType == null) {
			return null;
		}

		_availableArrowCount = Mathf.Clamp(_availableArrowCount - 1, 0, _maxArrowCount);
		NotifyArrowChanges();
		var arrow = _currentArrowType.ArrowScene.Instantiate<Arrow>();

		// TODO: Move into specific RecallQuiver -> normal quiver does not automatically get arrows back
		// arrow.Released += CurrentArrowOnReleased;
		arrow.Cancelled += ArrowOnCancelled;

		return arrow;
	}

	private void ArrowOnCancelled(Arrow activeArrow) {
		ReturnArrow(activeArrow);
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
