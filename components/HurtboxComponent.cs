using Godot;

public partial class HurtboxComponent : Area2D {
	[Signal]
	public delegate void HitEventHandler(HitboxComponent hitboxComponent, Vector2 direction);

	[Export] public HitflashComponent? HitflashComponent;
	[Export] public float HurtTime = 0.2f;
	[Export] public CollisionShape2D CollisionShape2D = null!;
	[Export] public float InvulnerableTime = 0.2f;
	[Export] public AnimatedSprite2D? Sprite;

	public override void _Ready() {
		AreaEntered += HandleCollision;
		BodyEntered += HandleCollision;
	}

	public void HandleCollision(Node2D node) {
		if (node is not HitboxComponent hitboxComponent || !hitboxComponent.CanDamage(this)) {
			return;
		}

		EmitSignal(SignalName.Hit, hitboxComponent, GlobalPosition.DirectionTo(hitboxComponent.GlobalPosition));
		
		if (InvulnerableTime > 0f) {
			CollisionShape2D.SetDeferred("disabled", true);

			var timer = GetTree().CreateTimer(InvulnerableTime);
			timer.Timeout += () => CollisionShape2D.SetDeferred("disabled", false);
		}

		if (Sprite != null && Sprite.SpriteFrames.HasAnimation("Hurt")) {
			Sprite.Play("Hurt");

			SceneTreeTimer? hurtTimer = GetTree().CreateTimer(HurtTime);
			hurtTimer.Timeout += () => Sprite.Play("Idle");
		}

		HitflashComponent?.Flash();
	}
}
