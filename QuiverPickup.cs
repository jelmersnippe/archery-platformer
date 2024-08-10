using Godot;

public partial class QuiverPickup : Pickup {
	private readonly PackedScene _quiverScene =
		ResourceLoader.Load<PackedScene>("res://quiver.tscn");

	protected override void Apply(Player player) {
		var quiver = _quiverScene.Instantiate<Quiver>();
		player.Equip(quiver);
	}
}
