using Sandbox;

public class BuildingComponent : InfoPanel
{
	[Property] public SteamId OwnerId { get; set; }
	[Property] public float Health { get; set; } = 100f;
	[Property] public float MaxHealth { get; set; } = 100f;
	protected override void OnUpdate()
	{
		base.OnUpdate();
	}
}