using Godot;

[GlobalClass]
public partial class BowUpgrade : Upgrade {
	private readonly PackedScene _bowScene =
		ResourceLoader.Load<PackedScene>("res://bow.tscn");

	public override void Apply(Player player) {
		var bow = _bowScene.Instantiate<Bow>();
		player.EquipBow(bow);
	}

	public override void Remove(Player player) {
		player.EquipBow(null);
	}
}
