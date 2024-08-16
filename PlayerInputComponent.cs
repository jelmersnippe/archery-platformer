using Godot;

public partial class PlayerInputComponent : Node {
	[Export] public bool Disabled;
		
	public Vector2 GetDirectionalInput() {
		if (Disabled) {
			return Vector2.Zero;
		}
		
		float verticalDirection = Input.GetAxis("move_up", "move_down");
		float horizontalDirection = Input.GetAxis("move_left", "move_right");
		return new Vector2(horizontalDirection, verticalDirection);
	}

	public bool IsActionJustPressed(string action) {
		return !Disabled && Input.IsActionJustPressed(action);
	}
	
	public bool IsActionJustReleased(string action) {
		return !Disabled && Input.IsActionJustReleased(action);
	}
}
