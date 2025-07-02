using Sandbox;
using Sandbox.Rendering;
using System;

public enum SelectableType
{
	Unit,
	Building,
	Resource,
	Other
}

public class Selectable : Component
{
	public string Name = "Selectable";
	public SelectableType _type = SelectableType.Other;
	public int _subType = 0;

	public bool _isSelected;
	public Color _selectionColor = Color.Green;
	public float Health = 100;
	public float MaxHealth = 100;
	public float Energy = 100;
	public float MaxEnergy = 100;

	public float MinRadius { get; set; } = 3f;
	public float MaxRadius { get; set; } = 8f;

	protected override void OnUpdate()
	{
		if ( !_isSelected || Scene.Camera is null )
			return;

		Vector3 elevatedPosition = GameObject.WorldPosition + Vector3.Up * 150f;

		float distance = Vector3.DistanceBetween( Scene.Camera.WorldPosition, GameObject.WorldPosition );

		// Inverse relation: radius gets smaller with distance (add epsilon to avoid divide-by-zero)
		float rawRadius = 10000f / (distance + 1f);

		// Clamp the radius to MinRadius and MaxRadius
		float radius = Math.Clamp( rawRadius, MinRadius, MaxRadius );

		Vector2 screenPos = Scene.Camera.PointToScreenPixels( elevatedPosition );

		HudPainter hud = Scene.Camera.Hud;

		// Define diamond corners
		Vector2 top = screenPos + new Vector2( 0, -radius );
		Vector2 right = screenPos + new Vector2( radius, 0 );
		Vector2 bottom = screenPos + new Vector2( 0, radius );
		Vector2 left = screenPos + new Vector2( -radius, 0 );

		float lineWidth = 2f;

		// Draw lines between corners to form a diamond
		hud.DrawLine( top, right, lineWidth, _selectionColor );
		hud.DrawLine( right, bottom, lineWidth, _selectionColor );
		hud.DrawLine( bottom, left, lineWidth, _selectionColor );
		hud.DrawLine( left, top, lineWidth, _selectionColor );
	}
}
