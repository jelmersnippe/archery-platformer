using Godot;
using System;

public partial class Rock : Node2D {
	[Export] public Trigger Trigger = null!;
	[Export] public Area2D Area = null!;
	
	public override void _Ready()
	{
		Area.AreaEntered += OnAreaEntered; 
	}

	private void OnAreaEntered(Area2D area) {
		GlobalTriggerState.SetTriggerState(Trigger, true);
	}
}
