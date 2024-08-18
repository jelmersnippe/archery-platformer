using Godot;

public partial class RecallQuiver : Quiver {
	[Export] public float AutoRecallDelay = 2.5f;

	public void Recall() {
		foreach (Arrow arrow in ActiveArrows) {
			// Don't use ReturnArrow here because can't modify collection while iterating
			AvailableArrowCount = Mathf.Clamp(AvailableArrowCount + 1, 0, MaxArrowCount);
			if (IsInstanceValid(arrow)) {
				arrow.QueueFree();
			}
		}

		ActiveArrows.Clear();
		NotifyArrowChanges();
	}

	protected override void CurrentArrowOnReleased(Arrow arrow) {
		ActiveArrows.Add(arrow);

		arrow.Released -= CurrentArrowOnReleased;
		arrow.Impacted += ArrowOnImpacted;
	}

	private void ArrowOnImpacted(Arrow arrow) {
		arrow.TreeExiting += () => ArrowOnTreeExiting(arrow);
	}

	private void ArrowOnTreeExiting(Arrow arrow) {
		if (!ActiveArrows.Contains(arrow)) {
			return;
		}

		ActiveArrows.Remove(arrow);
		SceneTreeTimer? timer = GetTree().CreateTimer(AutoRecallDelay);
		timer.Timeout += () => ReturnArrow(null);
	}
}
