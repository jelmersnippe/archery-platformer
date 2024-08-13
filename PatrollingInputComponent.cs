using Godot;

public partial class PatrollingInputComponent : InputComponent {
	[Export] public RayCast2D WallCheck = null!;
	[Export] public RayCast2D FloorCheck = null!;
	[Export] public Vector2I InitialDirection = new (1, 0);
	[Export] public float CheckDistance = 10f;

	private Vector2I _direction;

	public override void _Ready() {
		_direction = InitialDirection;
		WallCheck.TargetPosition = new Vector2(_direction.X, _direction.Y) * CheckDistance;
		FloorCheck.TargetPosition = new Vector2(_direction.X, 1) * CheckDistance;
	}
	
	public override Vector2 GetDirection() {
		if (WallCheck.IsColliding() || !FloorCheck.IsColliding()) {
			_direction = -_direction;
			WallCheck.TargetPosition = -WallCheck.TargetPosition;
			FloorCheck.TargetPosition = new Vector2(-FloorCheck.TargetPosition.X, FloorCheck.TargetPosition.Y) ;
		}

		return _direction;
	}
}

