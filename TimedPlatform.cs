using System.Security.Cryptography;
using Godot;

public partial class TimedPlatform : StaticBody2D {
	[Export] public float Lifetime = 2f;
	[Export] public bool Respawns = true;
	[Export] public float RespawnTime = 5f;
	[Export] public Area2D PlayerDetectionArea = null!;
	[Export] public CollisionShape2D CollisionShape2D = null!;
	[Export] public CanvasItem Display = null!;
	
	public override void _Ready()
	{
		SetActive(true);
	}

	private void PlayerDetectionAreaOnBodyEntered(Node2D body) {
		if (body is not Player player || player.Velocity.Y != 0f) {
			return;
		}

		Flash(Lifetime / 3f);
		var timer = GetTree().CreateTimer(Lifetime);
		timer.Timeout += LifetimeTimerOnTimeout;
	}

	private void LifetimeTimerOnTimeout() {
		SetActive(false);
		
		if (Respawns) {
			var timer = GetTree().CreateTimer(RespawnTime);
			timer.Timeout += RespawnTimerOnTimeout;
		}
	}

	private void Flash(float delay) {
		if (!Visible) {
			return;
		}

		var adjustedDelay = Mathf.Max(delay, 0.1f);
		var timer = GetTree().CreateTimer(adjustedDelay);
		timer.Timeout += () => {
			Display.SelfModulate = new Color(1f, 1f, 1f, 1f * 0.3f);
			
			var flashTimer = GetTree().CreateTimer(0.05f);
			flashTimer.Timeout += () =>Display.SelfModulate = new Color(1f, 1f, 1f, 1f);
			
			Flash(adjustedDelay /2f);
		};
	}

	private void RespawnTimerOnTimeout() {
		SetActive(true);
	}

	private void SetActive(bool active) {
		Visible = active;
		CollisionShape2D.SetDeferred("disabled", !active);
		if (active) {
			PlayerDetectionArea.BodyEntered += PlayerDetectionAreaOnBodyEntered;
		}
		else {
			PlayerDetectionArea.BodyEntered -= PlayerDetectionAreaOnBodyEntered;
		}
	}
}
