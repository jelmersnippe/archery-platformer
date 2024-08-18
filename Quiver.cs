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

	protected int MaxArrowCount;
	protected int AvailableArrowCount;
	protected Array<Arrow> ActiveArrows = new();

	public override void _Ready() {
		MaxArrowCount = InitialArrowCount;
		AvailableArrowCount = MaxArrowCount;

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
		EmitSignal(SignalName.ArrowCountChanged, AvailableArrowCount, MaxArrowCount);
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

	public Arrow? GetArrow() {
		if (AvailableArrowCount <= 0 || _currentArrowType == null) {
			return null;
		}

		AvailableArrowCount = Mathf.Clamp(AvailableArrowCount - 1, 0, MaxArrowCount);
		NotifyArrowChanges();
		var arrow = _currentArrowType.ArrowScene.Instantiate<Arrow>();

		arrow.Released += CurrentArrowOnReleased;
		arrow.Cancelled += ArrowOnCancelled;

		return arrow;
	}

	private void ArrowOnCancelled(Arrow activeArrow) {
		ReturnArrow(activeArrow);
	}

	public void ReturnArrow(Arrow? arrow) {
		if (arrow != null) {
			ActiveArrows.Remove(arrow);
		}

		AvailableArrowCount = Mathf.Clamp(AvailableArrowCount + 1, 0, MaxArrowCount);
		NotifyArrowChanges();
	}

	protected virtual void CurrentArrowOnReleased(Arrow arrow) {
	}

	public void Restock() {
		AvailableArrowCount = MaxArrowCount;
		NotifyArrowChanges();
	}
}
