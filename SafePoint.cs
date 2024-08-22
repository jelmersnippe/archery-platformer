using Godot;

public partial class SafePoint : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body) {
		if (body is not Player player) {
			return;
		}

		GD.Print("Safe point triggered at " + GlobalPosition);
		player.LastSafePoint = GlobalPosition;
	}
}
