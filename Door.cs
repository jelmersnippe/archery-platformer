using Godot;

public partial class Door : StaticBody2D {
	[Export] public Trigger Trigger;
	[Export] public CollisionShape2D CollisionShape2D;

	public override void _Ready() {
		SetOpened(GlobalTriggerState.GetTriggerState(Trigger));
		GlobalTriggerState.TriggerChanged += (trigger, b) => {
			if (trigger != Trigger) {
				return;
			}
			SetOpened(b);
		};
	}

	private void SetOpened(bool opened) {
		CollisionShape2D.SetDeferred("disabled", opened);
		Visible = !opened;
	}
}
