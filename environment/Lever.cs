using Godot;

public partial class Lever : Node2D {
	[Export] public Trigger Trigger = null!;
	[Export] public AnimationPlayer AnimationPlayer = null!;
	[Export] public Area2D ArrowDetection = null!;
	[Export] public Node2D StickingPoint = null!;
	[Export] public Sprite2D Sprite = null!;

	private bool _flipped;

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
		}

		// TODO: Reimplement with detection zone on arrow
		// arrow.Velocity = Vector2.Zero;
		// arrow.CallDeferred("reparent", StickingPoint);
		// arrow.TransitionedToStuck += (self, stuckArrow) => _arrow = stuckArrow;
		// arrow.Stick();
		// Flip(arrow);
	}

	private void Flip(Node2D arrow) {
		if (_flipped) {
			return;
		}

		_flipped = true;
		AnimationPlayer.Play("switch");
		GlobalTriggerState.SetTriggerState(Trigger, true);
	}
}
