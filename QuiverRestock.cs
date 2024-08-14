public partial class QuiverRestock : Upgrade {
	public override void Apply(Player player) {
		player.Quiver?.Restock();
	}
}
