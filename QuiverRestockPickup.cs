public partial class QuiverRestockPickup : Pickup {
	protected override void InteractableOnInteracted(Player player) {
		player.Quiver?.Restock();
	}
}
