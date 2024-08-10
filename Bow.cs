using Godot;

public partial class Bow : Node2D {
	[Export] public AnimationPlayer BowAnimationPlayer = null!;
	[Export] public Node2D FiringPoint = null!;
	[Export] public float MaxDrawTime = 1f;
	[Export] public float MinArrowVelocity = 400f;
	[Export] public float MaxArrowVelocity = 1000f;

	private float _drawTime;

	private Arrow? _currentArrow;

	public override void _PhysicsProcess(double delta) {
		if (_currentArrow == null) {
			_drawTime = 0f;
			return;
		}

		_drawTime = Mathf.Min(_drawTime + (float)delta, MaxDrawTime);
	}

	public void ReadyArrow(Arrow arrow) {
		arrow.Position = Vector2.Zero;
		FiringPoint.AddChild(arrow);
		BowAnimationPlayer.Play("draw");
		_currentArrow = arrow;
	}

	public void CancelArrow() {
		_currentArrow?.QueueFree();
		_currentArrow = null;
		BowAnimationPlayer.Stop();
	}

	public void ReleaseArrow() {
		if (_currentArrow == null) {
			return;
		}

		Vector2 velocity = GlobalPosition.DirectionTo(GetGlobalMousePosition()) *
						   Mathf.Lerp(MinArrowVelocity, MaxArrowVelocity, _drawTime / MaxDrawTime);
		_currentArrow.Release(velocity);

		_currentArrow = null;
		BowAnimationPlayer.Stop();
	}
}
