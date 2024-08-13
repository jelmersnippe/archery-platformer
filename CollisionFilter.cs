using Godot;

[GlobalClass]
public partial class CollisionFilter : Resource {
	[Export] public Vector2I AcceptedNormal;

	public bool Validate(KinematicCollision2D collision) {
		return collision.GetNormal() == new Vector2(Mathf.Sign(AcceptedNormal.X), Mathf.Sign(AcceptedNormal.Y));
	}
}
