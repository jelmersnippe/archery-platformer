using Godot;

public partial class Door : StaticBody2D {
	private Vector2 _initialPosition;
	[Export] public CollisionShape2D CollisionShape2D;
	[Export] public Trigger Trigger;

	public override void _EnterTree() {
		GlobalTriggerState.TriggerChanged += OnTriggerChanged;
	}

	public override void _Ready() {
		_initialPosition = GlobalPosition;
		SetOpened(GlobalTriggerState.GetTriggerState(Trigger), false);
	}

	public override void _ExitTree() {
		GlobalTriggerState.TriggerChanged -= OnTriggerChanged;
	}

	private void OnTriggerChanged(Trigger trigger, bool b) {
		if (trigger != Trigger) {
			return;
		}

		SetOpened(b);
	}

	private void SetOpened(bool opened, bool withAnimation = true) {
		Vector2 targetPosition = opened ? _initialPosition - new Vector2(0, 160) : _initialPosition;
		if (withAnimation) {
			Tween? tween = GetTree().CreateTween();

			tween.TweenProperty(this, "global_position", targetPosition, 2f)
				 .SetTrans(Tween.TransitionType.Bounce)
				 .SetEase(Tween.EaseType.Out);

			tween.Play();
		}
		else {
			GlobalPosition = targetPosition;
		}
	}
}
