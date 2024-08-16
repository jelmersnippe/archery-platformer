using Godot;

[GlobalClass]
public partial class QuiverUpgrade : Upgrade {
	private readonly PackedScene _quiverScene =
		ResourceLoader.Load<PackedScene>("res://quiver.tscn");

	public override void Apply(Player player) {
		var quiver = _quiverScene.Instantiate<Quiver>();
		player.EquipQuiver(quiver);
	}

	public override void Remove(Player player) {
		player.EquipQuiver(null);
	}
}
