using System.Linq;
using Godot;
using Godot.Collections;

public partial class UpgradeSystem : Node {
	private Player _player = null!;
	[Export] public Array<Upgrade> Upgrades = new();

	public override void _Ready() {
		_player = GetParent<Player>();
		foreach (Upgrade? upgrade in Upgrades) {
			if (GlobalTriggerState.GetTriggerState(upgrade.Trigger)) {
				upgrade.Apply(_player);
			}
		}

		GlobalTriggerState.TriggerChanged += TriggerChanged;
	}

	private void TriggerChanged(Trigger trigger, bool state) {
		Upgrade? matchingUpgrade = Upgrades.FirstOrDefault(x => x.Trigger == trigger);
		if (matchingUpgrade == null) {
			return;
		}

		switch (state) {
			case true:
				matchingUpgrade.Apply(_player);
				break;
			case false:
				matchingUpgrade.Remove(_player);
				break;
		}
	}
}
