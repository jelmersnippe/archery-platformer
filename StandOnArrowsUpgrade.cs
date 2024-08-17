using Godot;

[GlobalClass]
public partial class StandOnArrowsUpgrade : Upgrade {
	[Export(PropertyHint.Layers2DPhysics)] public uint ArrowCollisionLayer;

	public override void Apply(Player player) {
		player.SetCollisionMask(player.CollisionMask | ArrowCollisionLayer);
	}

	public override void Remove(Player player) {
		player.SetCollisionMask(player.CollisionMask & ~ArrowCollisionLayer);
	}
}
