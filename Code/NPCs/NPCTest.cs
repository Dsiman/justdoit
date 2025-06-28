using Sandbox;
using Sandbox.Citizen;
using System.Collections.Generic;
using System.Linq;

public sealed class TestNPC : Component
{
	private List<GameObject> _waypoints = new();
	private NavMeshAgent _agent;
	private Vector3 _targetPosition = Vector3.Zero;

	private Vector3 _lastPosition;
	private float _stuckTimer = 0f;

	private NPCState State { get; set; } = NPCState.Idle;

	private SkinnedModelRenderer _skinnedModelRenderer => GameObject.Components.Get<SkinnedModelRenderer>();

	public enum NPCState
	{
		None,
		Idle,
		Walking
	}

	protected override void OnAwake()
	{
		_skinnedModelRenderer.Parameters.Set( "special_movement_states", 2);
		_agent = GameObject.Components.Get<NavMeshAgent>();
		if ( _agent == null )
		{
			Log.Warning( "NavMeshAgent not found on GameObject." );
			return;
		}

		_waypoints = Scene.GetAllObjects( true )
			.Where( go => go.Tags.Has( "waypoint" ) )
			.ToList();

		_lastPosition = GameObject.WorldPosition;
	}

	protected override void OnUpdate()
	{
		if ( _agent == null || _waypoints.Count == 0 )
			return;

		switch ( State )
		{
			case NPCState.Idle:
				ChooseNewTarget();
				break;

			case NPCState.Walking:
				CheckIfReachedTarget();
				CheckIfStuck();
				break;
		}
	}

	private void ChooseNewTarget()
	{
		var index = Game.Random.Int( 0, _waypoints.Count - 1 );
		_targetPosition = _waypoints[index].WorldPosition;
		GameObject.WorldRotation = Rotation.LookAt( (_targetPosition - GameObject.WorldPosition).Normal, Vector3.Up );
		_agent.MoveTo( _targetPosition );
		_lastPosition = GameObject.WorldPosition;
		_stuckTimer = 0f;

		State = NPCState.Walking;
	}

	private void CheckIfReachedTarget()
	{
		if ( Vector3.DistanceBetween( GameObject.WorldPosition, _targetPosition ) < 5f )
		{
			_agent.Stop();
			State = NPCState.Idle;
		}
	}

	private void CheckIfStuck()
	{
		float distanceMoved = Vector3.DistanceBetween( GameObject.WorldPosition, _lastPosition );

		if ( distanceMoved < 1f )
		{
			_stuckTimer += Time.Delta;
			if ( _stuckTimer >= 1f )
			{
				Log.Info( "NPC was stuck, choosing new target." );
				_agent.Stop();
				State = NPCState.Idle;
			}
		}
		else
		{
			_stuckTimer = 0f;
			_lastPosition = GameObject.WorldPosition;
		}
	}
}