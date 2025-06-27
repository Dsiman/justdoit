using System.Diagnostics;
using System.Runtime.Serialization;
using Sandbox;

public sealed class TopDownPlayerController : Component
{
	[Property] public float MoveSpeed { get; set; } = 5.0f;
	[Property] public float RotationSpeed { get; set; } = 10.0f;
	[Property] public float CameraDistance { get; set; } = 10.0f;
	[Property] public float CameraHeight { get; set; } = 5.0f;
	private bool _isControllingNpc = false;

	protected override void OnAwake()
	{
	}

	protected override void OnUpdate()
	{
		if ( _isControllingNpc )
		{
		
			return;
		}

		if ( Input.Pressed( "Forward" ) || Input.Down( "Forward" ) )
		{
			var forward = GameObject.WorldRotation.Forward;
			var newPosition = GameObject.WorldPosition + forward * MoveSpeed * Time.Delta;
			GameObject.WorldPosition = newPosition;
			GameObject.WorldRotation = Rotation.Slerp(GameObject.WorldRotation, Rotation.LookAt(forward), RotationSpeed * Time.Delta);
		}
		if ( Input.Pressed( "Backward" ) || Input.Down( "Backward" ) )
		{

		}
		if ( Input.Pressed( "Left" ) || Input.Down( "Left" ) )
		{

		}
		if ( Input.Pressed( "Right" ) || Input.Down( "Right" ) )
		{

		}
		if ( Input.Pressed( "Jump" ) || Input.Down( "Jump" ) )
		{

		}
		if ( Input.Pressed( "Duck" ) || Input.Down( "Duck" ) )
		{

		}
		if ( Input.Pressed( "Walk" ) || Input.Down( "Walk" ) )
		{

		}
		if ( Input.Pressed( "Run" ) || Input.Down( "Run" ) )
		{

		}
	}

}