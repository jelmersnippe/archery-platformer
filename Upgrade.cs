using Godot;

[GlobalClass]
public abstract partial class Upgrade : Resource {
	public abstract void Apply(Player player);
}
