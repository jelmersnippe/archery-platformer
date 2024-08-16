using Godot;

public partial class Knockable : Node {
	[Export] public bool LockX;
	[Export] public bool LockY;
	private CharacterBody2D _body = null!;

	[Export] public float ControlLossTime = 0.5f;
	[Export] public float KnockbackRecovery = 150f;

	public float ControlLossTimeLeft { get; private set; }

	public override void _Ready()
	{
		_body = GetParent<CharacterBody2D>();
	}

	public override void _PhysicsProcess(double delta) {
		if (ControlLossTimeLeft <= 0f) {
			return;
		}

		ControlLossTimeLeft -= (float)delta;

		var velocity = new Vector2 {
			X = LockX ? _body.Velocity.X : Mathf.MoveToward(_body.Velocity.X, 0f, KnockbackRecovery * (float)delta),
			Y = LockY || _body.MotionMode == CharacterBody2D.MotionModeEnum.Grounded ? _body.Velocity.Y : Mathf.MoveToward(_body.Velocity.Y, 0f, KnockbackRecovery * (float)delta)
		};
		_body.Velocity = velocity;
	}

	public void ApplyKnockback(Vector2 force) {
		Vector2 adjustedForce = force;
		if (LockX) {
			adjustedForce.X = 0f;
		}
		if (LockY) 
		{
			adjustedForce.Y = 0f;
		}
		
		_body.Velocity = adjustedForce;
		ControlLossTimeLeft = ControlLossTime;
	}
}
