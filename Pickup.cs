using Godot;

public partial class Pickup : Node2D {
	[Export] public string Text = "pickup";
	[Export] public Texture2D Texture = null!;
	[Export] public Upgrade Upgrade = null!;
	[Export] public Interactable Interactable = null!;

	public override void _Ready() {
		Interactable.Sprite.Texture = Texture;
		Interactable.Text.Text = "E to " + Text;
		Interactable.Interacted += InteractableOnInteracted;
	}

	private void InteractableOnInteracted(Player player) {
		Upgrade.Apply(player);
	}
}
