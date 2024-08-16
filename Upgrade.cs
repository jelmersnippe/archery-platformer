using Godot;

[GlobalClass]
public abstract partial class Upgrade : Resource {
	[Export] public Trigger Trigger = null!;

	public abstract void Apply(Player player);
	public abstract void Remove(Player player);
}
