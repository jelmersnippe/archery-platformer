using Godot;
using System;

public partial class StuckArrow : StaticBody2D
{
	[Signal]
	public delegate void LifeTimeRanOutEventHandler();

	[Export] public float InitialLifeTime = 3f;
	[Export] public Area2D PlayerDetectionArea;

	private float _remainingLifeTime;

	private Player _trackingPlayer;
	
	public override void _Ready()
	{
		_remainingLifeTime = InitialLifeTime;
		PlayerDetectionArea.BodyEntered += PlayerDetectionAreaOnAreaEntered;
		PlayerDetectionArea.BodyExited += PlayerDetectionAreaOnBodyExited;
	}

	private void PlayerDetectionAreaOnBodyExited(Node2D body)
	{
		if (body == _trackingPlayer)
		{
			GD.Print("no longer tracking player");
			_trackingPlayer = null;
		}
	}

	private void PlayerDetectionAreaOnAreaEntered(Node2D area)
	{
		if (area is not Player player)
		{
			return;
		}

		GD.Print("tracking player");
		_trackingPlayer = player;
	}

	public override void _Process(double delta)
	{
		if (_trackingPlayer != null && _trackingPlayer.Velocity.Y == 0)
		{
			_remainingLifeTime -= (float)delta;
			GD.Print("reduced life time to " + _remainingLifeTime);
		}

		if (_remainingLifeTime <= 0)
		{
			GD.Print("life time ran out");
			EmitSignal(SignalName.LifeTimeRanOut);
			QueueFree();
		}
	}
}
