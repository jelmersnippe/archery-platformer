using Godot;

public partial class Door : StaticBody2D {
	[Export] public Trigger Trigger;
	[Export] public CollisionShape2D CollisionShape2D;

	public override void _EnterTree() {
		GlobalTriggerState.TriggerChanged += OnTriggerChanged;
	}

	public override void _Ready() {
		SetOpened(GlobalTriggerState.GetTriggerState(Trigger));
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

	private void SetOpened(bool opened) {
		var targetPosition = new Vector2(0, 160);
		var tween = GetTree().CreateTween();

		tween.TweenProperty(this, "position", Position + (isFlipped ? -targetPosition : targetPosition), 2f)
			 .SetTrans(Tween.TransitionType.Bounce)
			 .SetEase(Tween.EaseType.Out);
		
		tween.Play();
	}
}

