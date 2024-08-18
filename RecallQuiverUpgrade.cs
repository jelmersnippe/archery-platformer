using Godot;

[GlobalClass]
public partial class RecallQuiverUpgrade : Upgrade {
	private readonly PackedScene _quiverScene =
		ResourceLoader.Load<PackedScene>("res://recall_quiver.tscn");

	public override void Apply(Player player) {
		var quiver = _quiverScene.Instantiate<RecallQuiver>();
		player.EquipQuiver(quiver);
	}

	public override void Remove(Player player) {
		player.EquipQuiver(null);
	}
}
