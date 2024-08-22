using System.Security.Cryptography.X509Certificates;
using Godot;

public partial class PlayerInputComponent : Node {
	public bool Disabled { get; private set; }
	
	private float _remainingDisabledTime;
	private bool _onTimer = false;

	public void SetDisabled(bool disabled, float? time = null) {
		Disabled = disabled;
		GD.Print("setting input disabled " +disabled+ " for "  +time  );

		if (!disabled) {
			_remainingDisabledTime = 0f;
			_onTimer = false;
		}
		else if (time.HasValue) {
			_remainingDisabledTime = time.Value;
			_onTimer = true;
		}
	}

	public override void _Process(double delta) {
		if (!_onTimer) {
			return;
		}

		_remainingDisabledTime -= (float)delta;
		if (_remainingDisabledTime <= 0f) {
			SetDisabled(false);
		}
	}

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
