using Godot;

public partial class Lever : Node2D {
	[Signal]
	public delegate void FlippedEventHandler(bool isFlipped);
	
	[Export] public AnimationPlayer AnimationPlayer;
	[Export] public Area2D ArrowDetection;
	[Export] public Node2D StickingPoint;
	[Export] public Sprite2D Sprite;
	
	private bool _flipped;
	private Node2D _arrow;

	public override void _Ready() {
		Sprite.Frame = 0;
		ArrowDetection.BodyEntered += ArrowDetectionOnBodyEntered;
		AnimationPlayer.AnimationFinished += _ => AnimationPlayer.Stop(true);
	}

	private void ArrowDetectionOnBodyEntered(Node2D body) {
		if (body is not Arrow arrow) {
			return;
		}

		// Only allow collisions from left
		if (arrow.Velocity.X < 0) {
			return;
		}

		arrow.Velocity = Vector2.Zero;
		arrow.CallDeferred("reparent", StickingPoint);
		arrow.TransitionedToStuck += (self, stuckArrow) => _arrow = stuckArrow;
		arrow.Stick();
		Flip(arrow);
	}

	public override void _Process(double delta)
	{
		if (!_flipped) {
			return;
		}

		if (_arrow != null && IsInstanceValid(_arrow)) {
			return;
		}

		_flipped = false;
		AnimationPlayer.PlayBackwards("switch");
		EmitSignal(SignalName.Flipped, false);
	}

	private void Flip(Node2D arrow) {
		_arrow = arrow;
		_flipped = true;
		AnimationPlayer.Play("switch");
		EmitSignal(SignalName.Flipped, true);
	}
}
