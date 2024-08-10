using Godot;

public partial class TrajectoryLine : Line2D {
	[Export] public int PointCount = 120;

	public override void _Ready() {
		ClearPoints();
	}

	public void Update(Vector2 velocity, float gravity, float delta) {
		ClearPoints();

		Vector2 position = GlobalPosition;
		for (int i = 0; i < PointCount; i++) {
			AddPoint(ToLocal(position));
			velocity += new Vector2(0, gravity);
			position += velocity * delta;
		}
	}
}
