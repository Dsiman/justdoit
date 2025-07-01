using Sandbox;

public sealed class CabnetComponent : BuildingComponent
{
	
	protected override void OnAwake()
	{
		Name = "Tool Cabnet";
		_type = SelectableType.Building;
		_subType = 1;
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