using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

[StyleSheet( "Selected.scss" )]
public class Selected : Panel
{
	private SelectableType? _selectedType = null;
	private int? _selectedSubType = null;
	private List<GameObject> _selected = new();
	private List<SelectableCard> _selectableCards = new();
	private GameObject _player;
	private Stack<(SelectableType?, int?)> _history = new();

	private string _lastSelectionHash = "";
	private Panel _deck;
	private Panel _header;
	private bool _shiftGroupSelection = false;
	private List<GameObject> _shiftGroup = new();

	public Selected()
	{
		_deck = new Panel();
		_deck.Parent = this;
		_deck.AddClass( "deck" );

		_header = new Panel();
		_header.Parent = this;
		_header.AddClass( "header" );
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );
		FindPlayer();
	}

	public override void Tick()
	{
		base.Tick();
		UpdateSelected();
		HandleShiftGrouping();
	}

	private void FindPlayer()
	{
		if ( Scene.Directory is null ) return;

		_player = Scene.GetAllObjects( true )
			.Where( go => go.Tags.Has( "player" ) )
			.Where( x => !x.IsProxy )
			.FirstOrDefault();
	}

	private string ComputeSelectionHash( List<GameObject> selection )
	{
		if ( selection.Count == 0 ) return "";

		StringBuilder sb = new();
		foreach ( var obj in selection )
		{
			var sel = obj.GetComponent<Selectable>();
			if ( sel != null )
			{
				sb.Append( sel._type.ToString() );
				sb.Append( sel._subType.ToString() );
				sb.Append( obj.Name );
			}
		}
		return sb.ToString().GetHashCode().ToString();
	}

	private void UpdateCards()
	{
		_deck.DeleteChildren( true );
		_selectableCards.Clear();

		var validSelectables = _selected
			.Select( go => go.GetComponent<Selectable>() )
			.Where( sel => sel != null )
			.ToList();

		if ( !validSelectables.Any() ) return;

		// Check if all selectables are same type and subtype
		bool allSameType = validSelectables.All( sel =>
			sel._type == validSelectables[0]._type &&
			sel._subType == validSelectables[0]._subType );

		if ( allSameType && _selectedType == null && _selectedSubType == null )
		{
			_selectedType = validSelectables[0]._type;
			_selectedSubType = validSelectables[0]._subType;
		}

		// Grouping logic
		var groups = validSelectables
			.GroupBy( sel => sel._type )
			.OrderBy( g => g.Key );

		// TYPE LEVEL - Show one card per type
		if ( _selectedType == null )
		{
			foreach ( var typeGroup in groups )
			{
				var card = new SelectableCard();
				card.Parent = _deck;

				// Calculate group health/energy ratios
				float totalHealth = typeGroup.Sum( sel => sel.Health );
				float totalMaxHealth = typeGroup.Sum( sel => sel.MaxHealth );
				float healthRatio = totalMaxHealth > 0 ? totalHealth / totalMaxHealth : 0;

				float totalEnergy = typeGroup.Sum( sel => sel.Energy );
				float totalMaxEnergy = typeGroup.Sum( sel => sel.MaxEnergy );
				float energyRatio = totalMaxEnergy > 0 ? totalEnergy / totalMaxEnergy : 0;

				card.SetTypeGroup( typeGroup.Key, typeGroup.Count(), healthRatio, energyRatio );
				card.AddEventListener( "onmousedown", e => HandleCardClick( e as MousePanelEvent, card, typeGroup.Key, null ) );
				_selectableCards.Add( card );
			}
		}
		// SUBTYPE LEVEL - Show one card per subtype
		else if ( _selectedSubType == null )
		{
			var typeGroup = groups.FirstOrDefault( g => g.Key == _selectedType );
			if ( typeGroup != null )
			{
				var distinctSubtypes = typeGroup.Select( sel => sel._subType ).Distinct().ToList();

				// Skip subtype level if only one subtype exists
				if ( distinctSubtypes.Count == 1 )
				{
					_selectedSubType = distinctSubtypes.First();
					UpdateCards();
					return;
				}

				var subtypeGroups = typeGroup
					.GroupBy( sel => sel._subType )
					.OrderBy( g => g.Key );

				foreach ( var subtypeGroup in subtypeGroups )
				{
					var card = new SelectableCard();
					card.Parent = _deck;

					// Calculate group health/energy ratios
					float totalHealth = subtypeGroup.Sum( sel => sel.Health );
					float totalMaxHealth = subtypeGroup.Sum( sel => sel.MaxHealth );
					float healthRatio = totalMaxHealth > 0 ? totalHealth / totalMaxHealth : 0;

					float totalEnergy = subtypeGroup.Sum( sel => sel.Energy );
					float totalMaxEnergy = subtypeGroup.Sum( sel => sel.MaxEnergy );
					float energyRatio = totalMaxEnergy > 0 ? totalEnergy / totalMaxEnergy : 0;

					card.SetSubTypeGroup( subtypeGroup.Key, subtypeGroup.Count(), healthRatio, energyRatio );
					card.AddEventListener( "onmousedown", e => HandleCardClick( e as MousePanelEvent, card, null, subtypeGroup.Key ) );
					_selectableCards.Add( card );
				}
			}
		}
		// INDIVIDUAL LEVEL - Show all cards
		else
		{
			var typeGroup = groups.FirstOrDefault( g => g.Key == _selectedType );
			if ( typeGroup != null )
			{
				var subtypeGroup = typeGroup
					.Where( sel => sel._subType == _selectedSubType.Value )
					.OrderBy( sel => sel.GameObject.Name );

				foreach ( var sel in subtypeGroup )
				{
					var card = new SelectableCard();
					card.Parent = _deck;
					card.SetSelectable( sel );
					card.AddEventListener( "onmousedown", e => HandleCardClick( e as MousePanelEvent, card, null, null ) );
					_selectableCards.Add( card );
				}
			}
		}
	}

	private void HandleCardClick( MousePanelEvent e, SelectableCard card, SelectableType? type, int? subtype )
	{
		if ( e == null ) return;

		// Primary click (left mouse)
		if ( e.Button == "mouseleft" )
		{
			if ( type.HasValue )
			{
				_history.Push( (_selectedType, _selectedSubType) );
				_selectedType = type.Value;
				UpdateCards();
			}
			else if ( subtype.HasValue )
			{
				_history.Push( (_selectedType, _selectedSubType) );
				_selectedSubType = subtype.Value;
				UpdateCards();
			}
			else
			{
				// Open menu for individual selectable
				Log.Info( $"Opening menu for: {card._selectable.Name}" );
			}
		}
		// Secondary click (right mouse)
		else if ( e.Button == "mouseright" )
		{
			ShowQuickActions( card );
		}
		// Middle mouse
		else if ( e.Button == "mousemiddle" )
		{
			NavigateBack();
		}
	}

	private void ShowQuickActions( SelectableCard card )
	{
		Log.Info( $"Showing quick actions for: {card._name.Text}" );
		// Implement your quick actions UI here
	}

	private void NavigateBack()
	{
		if ( _history.Count > 0 )
		{
			var (prevType, prevSubType) = _history.Pop();
			_selectedType = prevType;
			_selectedSubType = prevSubType;
			UpdateCards();
		}
	}

	private void SelectHalf()
	{
		if ( _selectableCards.Count == 0 ) return;

		// Get selection tool
		var selectionTool = _player?.Components.Get<SelectionBoxTool>();
		if ( selectionTool == null ) return;

		// Clear current selection
		selectionTool.ClearSelection();

		// Select every other card
		for ( int i = 0; i < _selectableCards.Count; i += 2 )
		{
			if ( _selectableCards[i]._selectable != null )
			{
				selectionTool.AddToSelection( _selectableCards[i]._selectable.GameObject );
			}
		}
	}

	private void SendCommand()
	{
		Log.Info( "Sending command to selected units" );
		// Implement your command logic here
	}

	private void HandleShiftGrouping()
	{
		// Start shift group
		if ( Input.Pressed( "Shift" ) )
		{
			_shiftGroupSelection = true;
			_shiftGroup.Clear();
		}

		// Add to shift group when clicking while holding shift
		if ( _shiftGroupSelection && Input.Pressed( "Mouse1" ) )
		{
			var ray = Scene.Camera.ScreenPixelToRay( Mouse.Position );
			var tr = Scene.Trace.Ray( ray, 5000f ).WithTag( "selectable" ).Run();
			if ( tr.Hit )
			{
				_shiftGroup.Add( tr.GameObject );
			}
		}

		// Finalize shift group
		if ( _shiftGroupSelection && Input.Released( "Shift" ) )
		{
			var selectionTool = _player?.Components.Get<SelectionBoxTool>();
			if ( selectionTool != null )
			{
				selectionTool.ClearSelection();
				foreach ( var obj in _shiftGroup )
				{
					selectionTool.AddToSelection( obj );
				}
			}
			_shiftGroupSelection = false;
			_shiftGroup.Clear();
		}
	}

	// Reset selection when objects change
	private void UpdateSelected()
	{
		var selectionTool = _player?.Components.Get<SelectionBoxTool>();
		_selected = selectionTool?.Selected ?? new();

		string currentHash = ComputeSelectionHash( _selected );
		if ( currentHash != _lastSelectionHash )
		{
			_lastSelectionHash = currentHash;
			_selectedType = null;
			_selectedSubType = null;
			_history.Clear();
			UpdateCards();
		}
	}
}