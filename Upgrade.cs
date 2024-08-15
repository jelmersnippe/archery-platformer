using Godot;

[GlobalClass]
public abstract partial class Upgrade : Resource {
	[Export] public Trigger? Trigger;

	public void Use(Player player) {
		if (Trigger != null) {
			GlobalTriggerState.SetTriggerState(Trigger, true);
		}
		
		Apply(player);
	}
	
	protected abstract void Apply(Player player);
}
