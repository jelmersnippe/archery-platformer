using Godot;

[GlobalClass]
public partial class QuiverRestock : Upgrade {
	protected override void Apply(Player player) {
		player.Quiver?.Restock();
	}
}
