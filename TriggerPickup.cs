using Godot;

public partial class TriggerPickup : Pickup {
	[Export] public Trigger Trigger = null!;

	protected override void InteractableOnInteracted(Player player) {
		GlobalTriggerState.SetTriggerState(Trigger, true);
	}
}
