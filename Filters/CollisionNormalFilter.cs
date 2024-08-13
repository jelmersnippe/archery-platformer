using System.Linq;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class CollisionNormalFilter : CollisionFilter {
	[Export] public Array<Vector2I> AcceptedNormals = new();

	public override bool Validate(KinematicCollision2D collision) {
		var collisionNormal = collision.GetNormal();
		return AcceptedNormals.Any(x => new Vector2(Mathf.Sign(x.X), Mathf.Sign(x.Y)) == collisionNormal);
	}
}
