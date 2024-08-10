using Godot;

namespace ArcherPlatformer;

public partial class BowPickup : Pickup {
	private readonly PackedScene _bowScene =
		ResourceLoader.Load<PackedScene>("res://bow.tscn");

	protected override void Apply(Player player) {
		var bow = _bowScene.Instantiate<Bow>();
		player.Equip(bow);
	}
}
