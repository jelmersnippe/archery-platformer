using Godot;
using System;

public partial class Arrow : CharacterBody2D
{
	[Signal]
	public delegate void TransitionedToStuckEventHandler(StuckArrow stuckArrow);
	
	[Export] public PackedScene StuckArrowScene;
	
	public override void _PhysicsProcess(double delta)
	{
		var collision = MoveAndCollide(Velocity * (float)delta, true);
		if (collision != null)
		{
			var stuckArrow = StuckArrowScene.Instantiate<Node2D>();

			stuckArrow.Transform = Transform;
			GetParent().AddChild(stuckArrow);
			EmitSignal(SignalName.TransitionedToStuck, stuckArrow);
			QueueFree();
			return;
		}
		
		MoveAndSlide();
	}
}
