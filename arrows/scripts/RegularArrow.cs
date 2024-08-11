using Godot;

public partial class RegularArrow : Arrow {
	[Export] public float StuckArrowLifeTime;
	[Export] public PackedScene StuckArrowScene = null!;

	protected override void Impact(KinematicCollision2D collision) {
		Vector2 normal = collision.GetNormal();
		Sprite.AnimationFinished += () => SpawnStuckArrow(Mathf.Abs(normal.X) > Mathf.Abs(normal.Y));
	}

	private void SpawnStuckArrow(bool isSolid) {
		var stuckArrow = StuckArrowScene.Instantiate<StuckArrow>();
		stuckArrow.Transform = Transform;
		GetParent().CallDeferred("add_child", stuckArrow);

		if (isSolid) {
			stuckArrow.SetSolid(StuckArrowLifeTime);
		}

		QueueFree();
	}
}
