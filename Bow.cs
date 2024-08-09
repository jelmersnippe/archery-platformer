using Godot;
using Godot.Collections;

public partial class Bow : Node2D {
	[Export] public PackedScene ArrowScene = null!;
	[Export] public AnimationPlayer BowAnimationPlayer = null!;
	[Export] public Node2D FiringPoint = null!;
	[Export] public int InitialArrowCount = 5;

	private int _currentArrowCount;
	private Array<Node2D> _activeArrows = new();
	private Arrow? _currentArrow;

	public override void _Ready() {
		_currentArrowCount = InitialArrowCount;
	}

	public void ReadyArrow() {
		if (_currentArrowCount <= 0 || _currentArrow != null) {
			return;
		}

		_currentArrow = ArrowScene.Instantiate<Arrow>();
		_currentArrow.Position = Vector2.Zero;
		FiringPoint.AddChild(_currentArrow);
		BowAnimationPlayer.Play("draw");
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

		_currentArrowCount--;

		_currentArrow.Velocity = GlobalPosition.DirectionTo(GetGlobalMousePosition()) * 1000f;

		_currentArrow.CollisionShape2D.SetDeferred("disabled", false);
		_currentArrow.Reparent(GetTree().CurrentScene);

		_activeArrows.Add(_currentArrow);
		_currentArrow.TransitionedToStuck += (arrow, stuckArrow) => {
			_activeArrows.Remove(arrow);
			_activeArrows.Add(stuckArrow);
			stuckArrow.TreeExiting += () => {
				_activeArrows.Remove(stuckArrow);
				_currentArrowCount++;
			};
		};
		_currentArrow = null;
		BowAnimationPlayer.Stop();
	}

	public void Recall() {
		foreach (Node2D? arrow in _activeArrows) {
			arrow.QueueFree();
			_currentArrowCount++;
		}

		_activeArrows.Clear();
	}
}
