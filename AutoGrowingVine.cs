using Godot;

public partial class AutoGrowingVine : Area2D {
	[Export] public float GrowthSpeed;
	[Export] public Line2D Display = null!;
	[Export] public CollisionShape2D CollisionShape = null!;
	[Export] public RayCast2D RayCast = null!;

	private float _currentLength = 0;
	private bool _collided;

	private RectangleShape2D shape = new RectangleShape2D();

	public override void _Ready() {
		CollisionShape.Shape = shape;
	}

	public override void _Process(double delta) {
		if (_collided) {
			return;
		}
		
		_currentLength += GrowthSpeed * (float)delta;

		RayCast.TargetPosition = new Vector2(0, _currentLength);
		if (RayCast.IsColliding()) {
			_collided = true;
			return;
		}
		
		Display.Points = new[] {
			Vector2.Zero, new(0, _currentLength)
		};
		
		CollisionShape.Position = new Vector2(0, _currentLength / 2f);
		shape.Size = new Vector2(Display.Width, _currentLength);
	}
}
