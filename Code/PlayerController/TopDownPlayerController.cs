using System.Diagnostics;
using System.Runtime.Serialization;
using Sandbox;

[Title("Top Down Player")]
[Icon("Assist_Walker")]
public sealed class TopDownPlayerController : Component
{
	// Movement properties
	[Category("Movement Properties"), Property] public float BaseMoveSpeed { get; set; } = 200.0f;
	[Category("Movement Properties"), Property] public float FastMoveMultiplier { get; set; } = 3.0f;
	[Category("Movement Properties"), Property] public float SlowMoveMultiplier { get; set; } = 0.5f;
	[Category("Movement Properties"), Property] public float RotationSpeed { get; set; } = 50.0f;
	[Category("Movement Properties"), Property] public float VerticalMoveSpeed { get; set; } = 200.0f;
	
	// Camera properties
	[Category("Camera Properties"), Property] public float CameraDistance { get; set; } = 20.0f;
	[Category("Camera Properties"), Property] public float CameraHeight { get; set; } = 10.0f;
	[Category("Camera Properties"), Property] public float CameraFollowSpeed { get; set; } = 20.0f;
	[Category("Camera Properties"), Property] public float OrbitSensitivity { get; set; } = 0.2f;
	[Category("Camera Properties"), Property] public float PanSensitivity { get; set; } = 0.1f;
	
	// Mouse control properties
	[Category("Mouse Control Properties"), Property] public float MouseRotationSensitivity { get; set; } = .1f;
	[Category("Mouse Control Properties"), Property] public float DoubleClickThreshold { get; set; } = 0.3f;
	[Category("Mouse Control Properties"), Property] public float MinPitchAngle { get; set; } = 10.0f;
	[Category("Mouse Control Properties"), Property] public float MaxPitchAngle { get; set; } = 80.0f;
	
	private CameraComponent _camera => Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
	private bool _isControllingNpc = false;
	private bool _isOrbiting = false;
	private bool _isPanning = false;
	private Vector3 _cameraOffset = Vector3.Zero;
	private Vector2 _lastMousePosition;
	private TimeSince _lastMouse3Press;
	private float _currentMoveSpeed;
	private float _currentPitch = 45.0f;
	private float _currentYaw = 0.0f;

	protected override void OnAwake()
	{
		_currentMoveSpeed = BaseMoveSpeed;
		UpdateCameraPosition( true );
		GameObject.WorldPosition = new Vector3( -500, 0, 400 );
		GameObject.WorldRotation = Rotation.FromYaw(_currentYaw);
	}

	private void UpdateCameraPosition(bool immediate = true)
	{
		if (_camera == null)
			return;

		// Create camera rotation from player's yaw and camera's pitch
		var cameraRotation = Rotation.FromYaw(_currentYaw) * Rotation.FromPitch(_currentPitch);
		
		var targetPosition = GameObject.WorldPosition + Vector3.Up * CameraHeight - 
							cameraRotation.Forward * CameraDistance + _cameraOffset;
							
		if (immediate)
		{
			_camera.WorldPosition = targetPosition;
			_camera.WorldRotation = cameraRotation;
		}
		else
		{
			_camera.WorldPosition = Vector3.Lerp(_camera.WorldPosition, targetPosition, CameraFollowSpeed * Time.Delta);
			_camera.WorldRotation = Rotation.Slerp(_camera.WorldRotation, cameraRotation, CameraFollowSpeed * Time.Delta);
		}
		
		// Update player's rotation to match camera's yaw (horizontal rotation)
		GameObject.WorldRotation = Rotation.FromYaw(_currentYaw);
	}

	protected override void OnUpdate()
	{
		if (_isControllingNpc)
			return;

		// Calculate effective movement speed
		_currentMoveSpeed = BaseMoveSpeed;
		if (Input.Down("Shift"))
			_currentMoveSpeed *= FastMoveMultiplier;
		if (Input.Down("Alt"))
			_currentMoveSpeed *= SlowMoveMultiplier;

		// Get player's forward direction based on rotation
		var forward = GameObject.WorldRotation.Forward;
		var right = GameObject.WorldRotation.Right;

		// Movement input handling - now relative to player's rotation
		Vector3 movement = Vector3.Zero;

		if (Input.Down("W"))
			movement += forward;
		if (Input.Down("S"))
			movement -= forward;
		if (Input.Down("A"))
			movement -= right;
		if (Input.Down("D"))
			movement += right;

		// Apply movement
		if (movement.Length > 0)
		{
			movement = movement.Normal;
			GameObject.WorldPosition += movement * _currentMoveSpeed * Time.Delta;
		}

		// Rotation input handling - updates player's yaw
		if (Input.Down("Q"))
		{
			_currentYaw += RotationSpeed * Time.Delta;
		}
		if (Input.Down("E"))
		{
			_currentYaw -= RotationSpeed * Time.Delta;
		}

		// Vertical movement
		if (Input.Down("Space"))
		{
			GameObject.WorldPosition += Vector3.Up * VerticalMoveSpeed * Time.Delta;
		}
		if (Input.Down("CTRL"))
		{
			GameObject.WorldPosition -= Vector3.Up * VerticalMoveSpeed * Time.Delta;
		}

		// Mouse2 - Camera rotation/orbiting
		HandleMouseRotation();
		
		// Mouse3 - Camera panning and reset
		HandleCameraPanning();
		
		UpdateCameraPosition();
	}
	
	private void HandleMouseRotation()
	{
		if (Input.Pressed("Mouse2"))
		{
			_isOrbiting = true;
			_lastMousePosition = Sandbox.Mouse.Position;
			Sandbox.Mouse.Visibility = MouseVisibility.Hidden;
		}

		if (Input.Released("Mouse2"))
		{
			_isOrbiting = false;
			Sandbox.Mouse.Visibility = MouseVisibility.Visible;
		}
		
		if (_isOrbiting)
		{
			Vector2 mouseDelta = (Sandbox.Mouse.Position - _lastMousePosition) * MouseRotationSensitivity;
			_lastMousePosition = Sandbox.Mouse.Position;
			
			// Adjust player's yaw based on horizontal mouse movement
			_currentYaw += -mouseDelta.x;
			
			// Adjust camera's pitch based on vertical mouse movement
			_currentPitch -= -mouseDelta.y;
			
			// Clamp pitch to prevent flipping
			_currentPitch = MathX.Clamp(_currentPitch, MinPitchAngle, MaxPitchAngle);
		}
	}
	
	private void HandleCameraPanning()
	{
		// Double-click reset
		if (Input.Pressed("Mouse3"))
		{
			if (_lastMouse3Press < DoubleClickThreshold)
			{
				// Reset camera offset and angles
				_cameraOffset = Vector3.Zero;
				_currentPitch = 45.0f;
				_currentYaw = GameObject.WorldRotation.Yaw();
			}
			_lastMouse3Press = 0;
			_isPanning = true;
			_lastMousePosition = Sandbox.Mouse.Position;
		}

		if ( Input.Released( "Mouse3" ) )
		{
			_isPanning = false;
			Sandbox.Mouse.Visibility = MouseVisibility.Visible;
		}
		
		if (_isPanning)
		{	
			Sandbox.Mouse.Visibility = MouseVisibility.Hidden;
			Vector2 mouseDelta = (Sandbox.Mouse.Position - _lastMousePosition) * PanSensitivity;
			_lastMousePosition = Sandbox.Mouse.Position;
			
			// Convert screen movement to world space panning
			Vector3 panRight = -(_camera.WorldRotation.Right * mouseDelta.x);
			Vector3 panUp = -(_camera.WorldRotation.Up * mouseDelta.y);
			
			_cameraOffset += panRight + panUp;
		}
	}
}