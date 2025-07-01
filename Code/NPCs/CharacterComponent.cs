using Sandbox;

public sealed class CharacterComponent : InfoPanel
{

	protected override void OnAwake()
	{
		base.OnAwake();
		Name = "Default Unit Name";
		_type = SelectableType.Unit;
	}
	protected override void OnUpdate()
	{
		base.OnUpdate();
	}
}