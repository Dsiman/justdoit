using Sandbox;

public class Selectable : Component
{
	public bool _isSelected;
	public Color _selectionColor = Color.Green;
	protected override void OnUpdate()
	{
		if ( _isSelected )
		{
			var position = GameObject.WorldPosition + Vector3.Up * 150f;
			DebugOverlay.Box( position, Vector3.One * 15f, _selectionColor, 0.01f );
		}
	}
}