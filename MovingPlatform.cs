using Godot;

public partial class MovingPlatform : PathFollow2D {
	[Export] public float Speed = 50f;
	[Export] public float PauseTime = 1.5f;
	[Export] public AnimatableBody2D Body = null!;

	private bool _isPaused = true;
	private int _progressDirection = 1;

	private float _pathLength;
	private float _progressPerSecond;
	
	public override void _Ready() {
		ProgressRatio = 1f;
		_pathLength = Progress;
		ProgressRatio = 0f;
		var timeToComplete = _pathLength / Speed;
		_progressPerSecond = 100 / timeToComplete;
		GD.Print(_pathLength);
		GD.Print(timeToComplete);
		GD.Print(_progressPerSecond);
		
		Pause();
	}

	private void TimerOnTimeout() {
		_progressDirection = ProgressRatio >= 0.5f ? -1 : 1;
		_isPaused = false;
	}

	public override void _Process(double delta)
	{
		if (_isPaused) {
			return;
		}

		
		ProgressRatio += (_progressPerSecond * _progressDirection * (float)delta) / 100f;
		if (ProgressRatio >= 1f || ProgressRatio <= 0f) {
			Pause();
		}
	}

	private void Pause() {
		_isPaused = true;
		SceneTreeTimer? timer = GetTree().CreateTimer(PauseTime);
		timer.Timeout += TimerOnTimeout;
	}
}
