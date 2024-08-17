using Godot;

[GlobalClass]
public partial class WallMovementUpgrade : Upgrade {
	public override void Apply(Player player) {
		player.WallMovementAllowed = true;
	}

	public override void Remove(Player player) {
		player.WallMovementAllowed = false;
	}
}
