using Godot;

public partial class Bow : Node2D {
	[Export] public AnimationPlayer BowAnimationPlayer = null!;
	[Export] public Node2D FiringPoint = null!;
	[Export] public float MaxDrawTime = 1f;
	[Export] public float MinArrowVelocity = 400f;
	[Export] public float MaxArrowVelocity = 1000f;
	[Export] public TrajectoryLine TrajectoryLine = null!;

	private float _drawTime;

	public Arrow? CurrentArrow { get; private set; }

	private Vector2 ArrowVelocity => GlobalPosition.DirectionTo(GetGlobalMousePosition()) *
									 Mathf.Lerp(MinArrowVelocity, MaxArrowVelocity, _drawTime / MaxDrawTime);

	public override void _PhysicsProcess(double delta) {
		if (CurrentArrow == null) {
			_drawTime = 0f;
			return;
		}

		_drawTime = Mathf.Min(_drawTime + (float)delta, MaxDrawTime);
		TrajectoryLine.Update(ArrowVelocity, CurrentArrow.Gravity, (float)delta);
	}

	public void ReadyArrow(Arrow arrow) {
		FiringPoint.AddChild(arrow);
		arrow.Position = Vector2.Zero;
		BowAnimationPlayer.Play("draw");
		CurrentArrow = arrow;
	}

	public void CancelArrow() {
		CurrentArrow?.QueueFree();
		CurrentArrow = null;
		BowAnimationPlayer.Stop();
		TrajectoryLine.StopCalculating();
	}

	public void ReleaseArrow() {
		if (CurrentArrow == null) {
			return;
		}

		CurrentArrow.Release(ArrowVelocity);

		CurrentArrow = null;
		BowAnimationPlayer.Stop();
		TrajectoryLine.StopCalculating();
	}

	public bool HasArrowDrawn() {
		return CurrentArrow != null;
	}
}
