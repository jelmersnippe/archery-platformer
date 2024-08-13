using Godot;

[GlobalClass]
public abstract partial class CollisionFilter : Resource {
	public abstract bool Validate(KinematicCollision2D collision);
}
