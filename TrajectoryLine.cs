using Godot;

public partial class TrajectoryLine : Line2D {
	[Export] public int MaxPointCount = 10;
	[Export] public StaticBody2D CollisionBody = null!;

	private SegmentShape2D _segmentShape = null!;

	public override void _Ready() {
		ClearPoints();
	}

	public void StopCalculating() {
		ClearPoints();
	}

	public void Update(Vector2 velocity, float gravity, float delta) {
		ClearPoints();

		Vector2 position = GlobalPosition;
		CollisionBody.GlobalPosition = position;
		var pointCount = 0;
		while (pointCount < MaxPointCount) {
			Vector2 localPosition = ToLocal(position);
			AddPoint(localPosition);
			pointCount++;
			
			velocity += new Vector2(0, gravity);
			Vector2 toNextPoint = velocity * delta;

			var collision = CollisionBody.MoveAndCollide(toNextPoint);
			
			if (collision != null) {
				AddPoint(ToLocal(collision.GetPosition()));
				break;
			}
			
			position += toNextPoint;
		}
	}
}
