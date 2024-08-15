using Godot;

public partial class HealthComponent : Node {
	[Signal]
	public delegate void DiedEventHandler();

	[Signal]
	public delegate void HealthChangedEventHandler(int change, int current, int max);

	private int _currentHealth;
	private int _maxHealth;

	[Export] public int StartingHealth = 10;

	public override void _Ready() {
		_maxHealth = StartingHealth;
		_currentHealth = _maxHealth;
		NotifyHealth(0);
	}

	public void TakeDamage(int amount) {
		if (_currentHealth <= 0) {
			return;
		}

		int newHealth = Mathf.Clamp(_currentHealth - amount, 0, _maxHealth);
		int change = newHealth - _currentHealth;
		_currentHealth = newHealth;

		NotifyHealth(change);

		if (_currentHealth <= 0) {
			EmitSignal(SignalName.Died);
		}
	}

	private void NotifyHealth(int change) {
		EmitSignal(SignalName.HealthChanged, change, _currentHealth, _maxHealth);
	}
}
