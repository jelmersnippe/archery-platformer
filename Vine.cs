using Godot;

public partial class Vine : Area2D {
	[Export] public float Length;
	[Export] public Line2D Display;
	[Export] public CollisionShape2D CollisionShape;

	public override void _Ready() {
		Display.Points = new[] {
			Vector2.Zero, new(0, Length)
		};
		CollisionShape.Position = new Vector2(Display.Width, Length / 2f);
		var shape = new RectangleShape2D();
		shape.Size = new Vector2(Display.Width, Length);
		CollisionShape.Shape = shape;
	}
}
