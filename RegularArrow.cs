using Godot;

public partial class RegularArrow : Arrow {
	[Export] public float StuckArrowLifeTime;

	protected override void Impact(KinematicCollision2D collision) {
		Vector2 normal = collision.GetNormal();
		Sprite.AnimationFinished += () => SpawnStuckArrow(Mathf.Abs(normal.X) > Mathf.Abs(normal.Y));
	}

	private void SpawnStuckArrow(bool isSolid) {
		var stuckArrow = StuckArrowScene.Instantiate<StuckArrow>();

		if (isSolid) {
			stuckArrow.SetSolid(StuckArrowLifeTime);
		}

		stuckArrow.Transform = Transform;
		GetParent().CallDeferred("add_child", stuckArrow);
		QueueFree();
	}
}
