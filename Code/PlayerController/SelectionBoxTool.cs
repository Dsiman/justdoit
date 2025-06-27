using Sandbox;
using System;

public sealed class SelectionBoxTool : Component
{
	[Property] public List<GameObject> Selected { get; private set; } = new List<GameObject>(50);
	public CameraComponent Camera { get; private set; }
	private Vector3 _selectionBoxStart;
	private Vector3 _selectionBoxEnd;
	private bool _isSelecting;

	protected override void OnAwake()
	{
		Camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
		Sandbox.Mouse.Visibility = MouseVisibility.Visible;
	}


	protected override void OnUpdate()
	{
		HandleMouseInput();
	}

	private void HandleMouseInput()
	{
		if ( Input.Pressed( "Attack1" ) )
		{
			StartSelection();
		}
		else if ( Input.Down( "Attack1" ) && _isSelecting )
		{
			UpdateSelection();
		}
		else if ( Input.Released( "Attack1" ) && _isSelecting )
		{
			FinalizeSelection();
		}
	}

	private void StartSelection()
	{
		_isSelecting = true;

		var ray = Camera.ScreenPixelToRay( Mouse.Position );
		var tr = Scene.Trace.Ray( ray, 5000f ).WithTag( "world" ).Run();
		_selectionBoxStart = tr.HitPosition;
		_selectionBoxEnd = _selectionBoxStart;
		UpdateBoxVisual();
	}

	private void UpdateSelection()
	{
		var ray = Camera.ScreenPixelToRay( Mouse.Position );
		var tr = Scene.Trace.Ray( ray, 5000f ).WithTag( "world" ).Run();
		_selectionBoxEnd = tr.HitPosition;
		UpdateBoxVisual();
    	PreviewSelectionInBox();
	}

	private void FinalizeSelection()
	{
		SelectObjectsInBox();
		_isSelecting = false;
		_selectionBoxStart = Vector3.Zero;
		_selectionBoxEnd = Vector3.Zero;
	}

	private void UpdateBoxVisual()
	{
		var start = _selectionBoxStart;
		var end = _selectionBoxEnd;

		// Flatten Z to ground if desired
		var min = new Vector2( Math.Min( start.x, end.x ), Math.Min( start.y, end.y ) );
		var max = new Vector2( Math.Max( start.x, end.x ), Math.Max( start.y, end.y ) );
		var center = (min + max) / 2;
		var size = max - min;

		DebugOverlay.Box(
			new BBox(
				new Vector3( center.x - size.x / 2, center.y - size.y / 2, 0 ),
				new Vector3( center.x + size.x / 2, center.y + size.y / 2, 10 )
			),
			Color.Cyan.WithAlpha( 0.2f ),
			0.1f // Short time, refreshes each frame while dragging
		);
	}

	private void SelectObjectsInBox()
	{
		Selected.Clear();
		if ( !_isSelecting ) return;

		// Create a bounding box from start to end positions
		var min = new Vector3(
			Math.Min( _selectionBoxStart.x, _selectionBoxEnd.x ),
			Math.Min( _selectionBoxStart.y, _selectionBoxEnd.y ),
			Math.Min( _selectionBoxStart.z, _selectionBoxEnd.z )
		);

		var max = new Vector3(
			Math.Max( _selectionBoxStart.x, _selectionBoxEnd.x ),
			Math.Max( _selectionBoxStart.y, _selectionBoxEnd.y ),
			Math.Max( _selectionBoxStart.z, _selectionBoxEnd.z )
		);

		// Expand the Z range to capture objects at different heights
		min = min.WithZ( min.z - 50f );
		max = max.WithZ( max.z + 50f );

		var bounds = new BBox( min, max );

		// Find selectable objects
		var selectables = Scene.GetAllObjects( true )
			.Where( go => go.Tags.Has( "selectable" ) &&
						 bounds.Contains( go.WorldPosition ) );

		Selected.AddRange( selectables );
	}
	private void PreviewSelectionInBox()
	{
		var min = new Vector3(
			Math.Min(_selectionBoxStart.x, _selectionBoxEnd.x),
			Math.Min(_selectionBoxStart.y, _selectionBoxEnd.y),
			Math.Min(_selectionBoxStart.z, _selectionBoxEnd.z)
		);
		
		var max = new Vector3(
			Math.Max(_selectionBoxStart.x, _selectionBoxEnd.x),
			Math.Max(_selectionBoxStart.y, _selectionBoxEnd.y),
			Math.Max(_selectionBoxStart.z, _selectionBoxEnd.z)
		);

		// Expand Z range to capture vertically-stacked objects
		min = min.WithZ(min.z - 50f);
		max = max.WithZ(max.z + 50f);

		var bounds = new BBox(min, max);

		var selectables = Scene.GetAllObjects(true)
			.Where(go => go.Tags.Has("selectable") && bounds.Contains(go.WorldPosition));

		foreach (var obj in selectables)
		{
			DebugOverlay.Sphere(new Sphere(obj.WorldPosition, 20f), Color.Yellow.WithAlpha(0.4f), 0.1f);
		}
	}

}