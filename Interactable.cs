using Godot;

public partial class Interactable : Area2D {
	[Signal]
	public delegate void InteractedEventHandler(Player player);
		
	private readonly ShaderMaterial _outlineMaterialResource =
		ResourceLoader.Load<ShaderMaterial>("res://outline_material.tres");

	[Export] public Sprite2D Sprite = null!;
	[Export] public Label Text = null!;

	private ShaderMaterial? _outlineShader;

	public override void _Ready() {
		Text.Hide();
		SpawnPickupVisual();
	}

	public void Interact(Player player) {
		EmitSignal(SignalName.Interacted, player);
	}

	private void SpawnPickupVisual() {
		var outlineMaterial = _outlineMaterialResource.Duplicate() as ShaderMaterial;
		_outlineShader = outlineMaterial!;
		_outlineShader.SetShaderParameter("width", 0);

		Sprite.Material = _outlineShader;
		ShowInteractable(false);
	}

	public void ShowInteractable(bool interactable) {
		if (interactable) {
			_outlineShader?.SetShaderParameter("width", 1);
			Text.Show();
		}
		else {
			_outlineShader?.SetShaderParameter("width", 0);
			Text.Hide();
		}
	}
}
