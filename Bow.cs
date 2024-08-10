using Godot;

public partial class Bow : Node2D {
	[Export] public AnimationPlayer BowAnimationPlayer = null!;
	[Export] public Node2D FiringPoint = null!;
	[Export] public float MaxDrawTime = 1f;
	[Export] public float MinArrowVelocity = 400f;
	[Export] public float MaxArrowVelocity = 1000f;
	[Export] public TrajectoryLine TrajectoryLine = null!;

	private float _drawTime;

	private Arrow? _currentArrow;

	private Vector2 ArrowVelocity => GlobalPosition.DirectionTo(GetGlobalMousePosition()) *
									 Mathf.Lerp(MinArrowVelocity, MaxArrowVelocity, _drawTime / MaxDrawTime);

	public override void _PhysicsProcess(double delta) {
		if (_currentArrow == null) {
			_drawTime = 0f;
			return;
		}

		_drawTime = Mathf.Min(_drawTime + (float)delta, MaxDrawTime);
		TrajectoryLine.Update(ArrowVelocity, _currentArrow.Gravity, (float)delta);
	}

	public void ReadyArrow(Arrow arrow) {
		FiringPoint.AddChild(arrow);
		arrow.Position = Vector2.Zero;
		BowAnimationPlayer.Play("draw");
		_currentArrow = arrow;
	}

	public void CancelArrow() {
		_currentArrow?.QueueFree();
		_currentArrow = null;
		BowAnimationPlayer.Stop();
		TrajectoryLine.ClearPoints();
	}

	public void ReleaseArrow() {
		if (_currentArrow == null) {
			return;
		}

		_currentArrow.Release(ArrowVelocity);

		_currentArrow = null;
		BowAnimationPlayer.Stop();
		TrajectoryLine.ClearPoints();
	}
}
