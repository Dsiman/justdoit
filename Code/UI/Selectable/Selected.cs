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

	private string _lastSelectionHash = "";
	private Panel _deck;
	private Panel _header;

    public Selected()
	{
		_deck = new Panel();
		_deck.Parent = this;

		_header = new Panel();
		_header.Parent = this;
		_header.AddClass( "header" );

		var backButton = new Button( "Back" );
		backButton.Parent = _header;
		backButton.AddEventListener( "onclick", () =>
		{
			if ( _selectedSubType != null ) _selectedSubType = null;
			else if ( _selectedType != null ) _selectedType = null;
			UpdateCards();
		} );
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
        _deck.DeleteChildren(true);
        _selectableCards.Clear();

        var validSelectables = _selected
            .Select(go => go.GetComponent<Selectable>())
            .Where(sel => sel != null)
            .ToList();

        if (!validSelectables.Any()) return;

        // Grouping logic
        var groups = validSelectables
            .GroupBy(sel => sel._type)
            .OrderBy(g => g.Key);

        // TYPE LEVEL - Show one card per type
        if (_selectedType == null)
        {
            foreach (var typeGroup in groups)
            {
                var card = new SelectableCard();
                card.Parent = _deck;
                card.SetTypeGroup(typeGroup.Key, typeGroup.Count());
                card.AddEventListener("onclick", () => {
                    _selectedType = typeGroup.Key;
                    UpdateCards();
                });
                _selectableCards.Add(card);
            }
        }
        // SUBTYPE LEVEL - Show one card per subtype
        else if (_selectedSubType == null)
        {
            var typeGroup = groups.FirstOrDefault(g => g.Key == _selectedType);
            if (typeGroup != null)
            {
                var subtypeGroups = typeGroup
                    .GroupBy(sel => sel._subType)
                    .OrderBy(g => g.Key);

                foreach (var subtypeGroup in subtypeGroups)
                {
                    var card = new SelectableCard();
                    card.Parent = _deck;
                    card.SetSubTypeGroup(subtypeGroup.Key, subtypeGroup.Count());
                    card.AddEventListener("onclick", () => {
                        _selectedSubType = subtypeGroup.Key;
                        UpdateCards();
                    });
                    _selectableCards.Add(card);
                }
            }
        }
        // INDIVIDUAL LEVEL - Show all cards
        else
        {
            var typeGroup = groups.FirstOrDefault(g => g.Key == _selectedType);
            if (typeGroup != null)
            {
                var subtypeGroup = typeGroup
                    .Where(sel => sel._subType == _selectedSubType.Value)
                    .OrderBy(sel => sel.GameObject.Name);

                foreach (var sel in subtypeGroup)
                {
                    var card = new SelectableCard();
                    card.Parent = _deck;
                    card.SetSelectable(sel);
                    _selectableCards.Add(card);
                }
            }
        }

        ApplyFanLayout();
    }

    private void ApplyFanLayout()
    {
        int count = _selectableCards.Count;
        float maxRotation = 15f;
        float maxOffset = 30f;
        float spacing = count > 1 ? MathX.Clamp(100f / count, 20f, 40f) : 0f;

        for (int i = 0; i < count; i++)
        {
            var card = _selectableCards[i];
            float ratio = count > 1 ? i / (float)(count - 1) : 0.5f;
            float rotation = MathX.Lerp(-maxRotation, maxRotation, ratio);
            float yOffset = MathX.Lerp(maxOffset, 0, 1f - Math.Abs(ratio - 0.5f) * 2f);

            card.Style.Set("transform", $"rotate({rotation}deg) translateY({yOffset}px)");
            card.Style.Set("z-index", $"{i}");
        }
    }

    // Reset selection when objects change
    private void UpdateSelected()
    {
        var selectionTool = _player?.Components.Get<SelectionBoxTool>();
        _selected = selectionTool?.Selected ?? new();

        string currentHash = ComputeSelectionHash(_selected);
        if (currentHash != _lastSelectionHash)
        {
            _lastSelectionHash = currentHash;
            _selectedType = null;
            _selectedSubType = null;
            UpdateCards();
        }
    }
}