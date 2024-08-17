using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ArcheryUpgrade : Upgrade {
	[Export] public Array<Player.MovementState> ArcheryStates = new() {
		Player.MovementState.Airborne,
		Player.MovementState.WallGrab,
		Player.MovementState.WallSlide
	};

	public override void Apply(Player player) {
		player.CanUseArcheryStates.AddRange(ArcheryStates);
	}

	public override void Remove(Player player) {
		foreach (Player.MovementState state in ArcheryStates) {
			player.CanUseArcheryStates.Remove(state);
		}
	}
}
