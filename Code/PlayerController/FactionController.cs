using Sandbox;
using System;

public sealed class FactionController : Component
{
	// FactionController is responsible for managing the player's camera controls and selection of units and buildings.

	// Selected are lists that hold references to the currently selected units and buildings.
	// These lists can be used to perform actions on the selected entities, such as issuing commands or taking control of a unit.
	// The lists are initialized as empty and can be populated during gameplay when the player selects units or buildings.
	[Property] public List<GameObject> Selected { get; private set; } = new List<GameObject>();
	[Property] public CameraComponent Camera { get; private set; } = null;
	// Mouse Selection
	public Transform SelectionBoxStart { get; private set; }
	public Transform SelectionBoxEnd { get; private set; }


	// OnAwake is called when the component is initialized. It can be used to set up initial state or subscribe to events.
	protected override void OnAwake()
	{
		Camera = Scene.Directory.FindByName( "Camera" ).First().GetComponent<CameraComponent>();
		if ( Camera == null )
		{
			Log.Error( "FactionController: CameraComponent not found in the scene." );
			return;
		}
		// Make curser visible.
		Sandbox.Mouse.Visibility = MouseVisibility.Visible;
	}

	// OnUpdate is called every frame and can be used to handle input or update the state of the faction controller.
	protected override void OnUpdate()
	{
		HandleMouseInput();
		// TODO: Handle keyboard input for camera movement and selection.
		// TODO: Handle Transition From FactionController to UnitController. UnitController will be using default character controller for movement and actions.
	}

	private void HandleMouseInput()
	{
		// Handle mouse input for selection box
		// Using:
		// Input.Pressed( "Attack1" ) Cast a ray trace from the camera to the ground using the mouse position. and use that as the start position of the selection box.
		// Input.Down( "Attack1" ) Make sure to activly update the end position of the selection box as the mouse moves.
		// Input.Released( "Attack1" ) When fishing the selection box, process the selection box to select units or buildings within the box.
		if ( Input.Pressed( "Attack1" ) )
		{
			// Start the selection box at the current mouse position.
			
		}
		//
		
	}

	private void ProcessSelectionBox()
	{
	}
	
	private void DrawSelectionBox()
	{
		// Process the selection box to select units or buildings within the box using the world position.
		if ( SelectionBoxStart == default || SelectionBoxEnd == default )
			return;
		var start = SelectionBoxStart.Position;
		var end = SelectionBoxEnd.Position;
		var min = new Vector2( Math.Min( start.x, end.x ), Math.Min( start.y, end.y ) );
		var max = new Vector2( Math.Max( start.x, end.x ), Math.Max( start.y, end.y ) );
		var center = (min + max) / 2;
		var size = max - min;
		DebugOverlay.Box(
			new BBox(
				new Vector3( center.x - size.x / 2, center.y - size.y / 2, 0 ),
				new Vector3( center.x + size.x / 2, center.y + size.y / 2, 10 )
			),
			Color.Red.WithAlpha( 0.5f ),
			0.5f
		);
	}
}