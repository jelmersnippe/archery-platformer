using Godot;

public partial class PathFollowPlatform : PathFollow2D {
	[Export] public float Speed = 50f;
	[Export] public float PauseTime = 1.5f;
	[Export] public bool WaitForPlayer = false;
	[Export] public Area2D? PlayerDetectionArea;
	[Export] public Node2D Platform = null!;

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
		
		Pause();

		if (WaitForPlayer) {
			if (PlayerDetectionArea == null) {
				GD.PrintErr("MovingPlatform " + Name + " is set to wait for player but has no detection zone");
			}
			else {
				PlayerDetectionArea.BodyEntered += PlayerDetectionAreaOnBodyEntered;
			}
		}

		var remoteTransform2D = new RemoteTransform2D();
		remoteTransform2D.RemotePath = Platform.GetPath();
		
		AddChild(remoteTransform2D);
	}

	private void PlayerDetectionAreaOnBodyEntered(Node2D body) {
		if (body is not Player) {
			return;
		}

		_isPaused = false;
	}

	private void TimerOnTimeout() {
		if (!WaitForPlayer || PlayerDetectionArea == null) {
			_isPaused = false;
		}
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
		_progressDirection = ProgressRatio >= 0.5f ? -1 : 1;
		SceneTreeTimer? timer = GetTree().CreateTimer(PauseTime);
		timer.Timeout += TimerOnTimeout;
	}
}
