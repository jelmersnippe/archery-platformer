using Godot;

public partial class Lever : Node2D {
	[Export] public Trigger Trigger;
	[Export] public AnimationPlayer AnimationPlayer;
	[Export] public Area2D ArrowDetection;
	[Export] public Node2D StickingPoint;
	[Export] public Sprite2D Sprite;
	[Export] public bool ResetOnArrowRemove;

	private bool _flipped;
	private Node2D _arrow;

	public override void _Ready() {
		Sprite.Frame = 0;
		ArrowDetection.BodyEntered += ArrowDetectionOnBodyEntered;
		AnimationPlayer.AnimationFinished += _ => AnimationPlayer.Stop(true);

		_flipped = GlobalTriggerState.GetTriggerState(Trigger);

		if (!_flipped) {
			return;
		}

		float switchAnimationLength = AnimationPlayer.GetAnimation("switch").Length;
		AnimationPlayer.CurrentAnimation = "switch";
		AnimationPlayer.Seek(switchAnimationLength, true);
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

	public override void _Process(double delta) {
		if (!ResetOnArrowRemove) {
			return;
		}

		if (!_flipped) {
			return;
		}

		if (_arrow != null && IsInstanceValid(_arrow)) {
			return;
		}

		_flipped = false;
		AnimationPlayer.PlayBackwards("switch");
		GlobalTriggerState.SetTriggerState(Trigger, false);
	}

	private void Flip(Node2D arrow) {
		if (_flipped) {
			return;
		}

		_arrow = arrow;
		_flipped = true;
		AnimationPlayer.Play("switch");
		GlobalTriggerState.SetTriggerState(Trigger, true);
	}
}
