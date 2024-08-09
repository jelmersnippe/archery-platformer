using Godot;

public abstract partial class Pickup : Area2D {
	private readonly ShaderMaterial _outlineMaterialResource =
		ResourceLoader.Load<ShaderMaterial>("res://outline_material.tres");

	[Export] public Trigger? SingleUseTrigger;
	[Export] public Sprite2D Sprite = null!;
	[Export] public Label Text = null!;

	public ShaderMaterial? OutlineShader { get; private set; }

	public override void _Ready() {
		if (SingleUseTrigger != null && GlobalTriggerState.GetTriggerState(SingleUseTrigger)) {
			QueueFree();
			return;
		}

		AreaEntered += OnBodyEntered;
		AreaExited += OnBodyExited;

		Text.Hide();
		SpawnPickupVisual();
	}

	private void OnBodyExited(Node2D body) {
		ShowInteractable(false);
	}

	private void OnBodyEntered(Node2D body) {
		ShowInteractable(true);
	}

	public virtual void Grab(Player player) {
		if (SingleUseTrigger != null) {
			GlobalTriggerState.SetTriggerState(SingleUseTrigger, true);
		}

		QueueFree();
	}

	private void SpawnPickupVisual() {
		var outlineMaterial = _outlineMaterialResource.Duplicate() as ShaderMaterial;
		OutlineShader = outlineMaterial!;
		OutlineShader.SetShaderParameter("width", 0);

		Sprite.Material = OutlineShader;
		ShowInteractable(false);
	}

	private void ShowInteractable(bool interactable) {
		if (interactable) {
			OutlineShader?.SetShaderParameter("width", 1);
			Text.Show();
		}
		else {
			OutlineShader?.SetShaderParameter("width", 0);
			Text.Hide();
		}
	}
}
