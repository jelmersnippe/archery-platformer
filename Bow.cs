using Godot;

public partial class Bow : Node2D {
	[Export] public AnimationPlayer BowAnimationPlayer = null!;
	[Export] public Node2D FiringPoint = null!;
	private Arrow? _currentArrow;

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

		Vector2 velocity = GlobalPosition.DirectionTo(GetGlobalMousePosition()) * 1000f;
		_currentArrow.Release(velocity);

		_currentArrow = null;
		BowAnimationPlayer.Stop();
	}
}
