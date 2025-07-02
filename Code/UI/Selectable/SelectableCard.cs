using Sandbox;
using Sandbox.UI;
using System;

[StyleSheet( "SelectableCard.scss" )]
public class SelectableCard : Panel
{
	public Selectable _selectable;
	public Label _name { get; set; }
	public Model _iconModel;
	public SelectableType group;
	public Panel _healthBar;
	public Panel _healthBarBg;
	public Panel _energyBar;
	public Panel _energyBarBg;
	public Panel _statsPanel;
	public Panel _texturePanel;
	private RealTimeSince _lastFlash;
	private bool _flashing;
	private Color _healthColor = Color.Green;
	private Texture _modelTexture;
	private RealTimeSince _lastRenderTime = 0;
	private bool _textureRequested = false;



	public SelectableCard()
	{
		_name = new Label();
		_name.Parent = this;
		_name.AddClass( "card-name" );

		_statsPanel = new Panel();
		_statsPanel.Parent = this;
		_statsPanel.AddClass( "stats-panel" );

		// Health bar
		_healthBarBg = new Panel();
		_healthBarBg.Parent = _statsPanel;
		_healthBarBg.AddClass( "health-bg" );

		_healthBar = new Panel();
		_healthBar.Parent = _healthBarBg;
		_healthBar.AddClass( "health-bar" );

		// Energy bar
		_energyBarBg = new Panel();
		_energyBarBg.Parent = _statsPanel;
		_energyBarBg.AddClass( "energy-bg" );

		_energyBar = new Panel();
		_energyBar.Parent = _energyBarBg;
		_energyBar.AddClass( "energy-bar" );

		_texturePanel = new Panel();
		_texturePanel.Parent = this;
		_texturePanel.AddClass( "texture-panel" );

		_lastFlash = 0;
	}


	public void SetSelectable( Selectable selectable )
	{
	 this._selectable = selectable;
		if ( _selectable != null )
		{
			group = _selectable._type;
			UpdateStats();

			// Request texture generation
			_textureRequested = false;
		}
	}


	public void UpdateStats()
	{
		if ( _selectable != null )
		{
			_name.Text = _selectable.Name;
      _texturePanel.Style.Set( "background-image", "url('https://pngimg.com/uploads/shrek/shrek_PNG3.png')" );
			float healthRatio = Math.Clamp( _selectable.Health / _selectable.MaxHealth, 0f, 1f );
			_healthBar.Style.Width = Length.Percent( healthRatio * 100 );

			// Update health bar color and flashing
			UpdateHealthBarEffect( healthRatio );

			// Update energy bar
			float energyRatio = Math.Clamp( _selectable.Energy / _selectable.MaxEnergy, 0f, 1f );
			_energyBar.Style.Width = Length.Percent( energyRatio * 100 );
		}
	}

	private void UpdateHealthBarEffect( float healthRatio )
	{
		if ( healthRatio <= 0.15f )
		{
			// Solid red for critical health
			_healthColor = Color.Red;
			_flashing = false;
		}
		else if ( healthRatio <= 0.5f )
		{
			// Flashing red for low health
			_flashing = true;

			// Calculate flash speed based on health
			float flashSpeed = healthRatio <= 0.3f ? 0.2f : 0.5f;

			// Flash effect
			if ( _lastFlash > flashSpeed )
			{
				_healthColor = _healthColor == Color.Red ? new Color( 0.8f, 0, 0 ) : Color.Red;
				_lastFlash = 0;
			}
		}
		else
		{
			// Solid green for healthy
			_healthColor = Color.Green;
			_flashing = false;
		}

		// Apply color
		_healthBar.Style.BackgroundColor = _healthColor;
	}

	public void SetTypeGroup( SelectableType type, int count, float healthRatio, float energyRatio )
	{
		_name.Text = $"{type}\n[{count}]";
    switch ( type )
    {
      case SelectableType.Unit:
        _texturePanel.Style.Set( "background-image", "url('https://upload.wikimedia.org/wikipedia/commons/6/63/Icon_Bird_512x512.png')" );
        break;
      case SelectableType.Building:
        _texturePanel.Style.Set( "background-image", "url('https://upload.wikimedia.org/wikipedia/commons/c/cc/Icon_Pinguin_1_512x512.png')" );
        break;
    }
		// Set group health/energy ratios
		_healthBar.Style.Width = Length.Percent( healthRatio * 100 );
		_energyBar.Style.Width = Length.Percent( energyRatio * 100 );

		// Update health bar effect
		UpdateHealthBarEffect( healthRatio );
	}

	public void SetSubTypeGroup( int subType, int count, float healthRatio, float energyRatio )
	{
		_name.Text = $"{subType}\n[{count}]";
		// Set group health/energy ratios
		_healthBar.Style.Width = Length.Percent( healthRatio * 100 );
		_energyBar.Style.Width = Length.Percent( energyRatio * 100 );

		// Update health bar effect
		UpdateHealthBarEffect( healthRatio );
	}

	public override void Tick()
	{
		base.Tick();

		if ( _selectable != null )
		{
			UpdateStats();

			// Only attempt to create texture once per second if not created
			if ( _modelTexture == null && !_textureRequested && _lastRenderTime > 1.0f )
			{
				_textureRequested = true;
				_lastRenderTime = 0;
			}
		}
	}
}