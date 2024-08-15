using Godot;

public partial class ConditionalNode : Node {
	[Export] public Trigger Trigger = null!;
	[Export] public bool ExpectedState;

	public override void _Ready() {
		if (GlobalTriggerState.GetTriggerState(Trigger) != ExpectedState) {
			QueueFree();
			return;
		}
		
		GlobalTriggerState.TriggerChanged += TriggerChanged;
	}

	private void TriggerChanged(Trigger trigger, bool state) {
		if (trigger != Trigger) {
			return;
		}

		if (state != ExpectedState) {
			QueueFree();
		}
	}
}
