using Sandbox;

public sealed class SelectableBuilding : Component
{
	public bool _isSelected;
	protected override void OnUpdate()
	{
		// if isSelected is true, put a diamond above the unit
		if ( _isSelected )
		{
			var position = GameObject.WorldPosition + Vector3.Up * 50f;
			DebugOverlay.Box(position, Vector3.One * 10f, Color.Red, 0.01f);
		}

	}
}