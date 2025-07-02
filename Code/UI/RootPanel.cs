using Sandbox;


public sealed class RootPanel : PanelComponent
{
	Selected selectedPanel;


	protected override void OnTreeFirstBuilt()
	{
		base.OnTreeFirstBuilt();

		selectedPanel = new Selected();
		selectedPanel.Parent = Panel;

	}

}