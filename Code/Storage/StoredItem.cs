using Sandbox;

public class StoredItem
{
	public Model Previewmodel;
	public Texture texture;
	public ItemShape itemShape;
	public GameObject itemToStore;

	// Placement information
	public IntVector2 Position { get; set; }
	public int Rotation { get; set; }
}
