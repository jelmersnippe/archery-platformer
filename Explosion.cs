using Godot;
using System;

public partial class Explosion : Area2D {
	[Export] public AnimationPlayer AnimationPlayer = null!;
	
	public override void _Ready() {
		AnimationPlayer.AnimationFinished += name => QueueFree();
	}
}
