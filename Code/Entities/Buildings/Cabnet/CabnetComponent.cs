using Sandbox;

public sealed class CabnetComponent : BuildingComponent
{
	
	protected override void OnAwake()
	{
		_selectionColor = Color.Blue;
		base.OnAwake();
	}

	protected override void OnUpdate()
	{
		if ( _isSelected )
		{
			
		}
		else
		{

		}

		base.OnUpdate();
	}
}