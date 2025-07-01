using Sandbox.UI;
using System;

[StyleSheet("SelectableCard.scss")]
public class SelectableCard : Panel
{
  public Selectable _selectable;
  public Label _name { get; set; }
  public Button _button { get; set; }
  public Model _iconModel;
  public SelectableType group;



  public SelectableCard()
  {
    _name = new Label();
    _name.Parent = this;
    _button = new Button();
    _button.Parent = this;
  }
  public void SetSelectable(Selectable selectable)
  {
    this._selectable = selectable;
    if (_selectable != null)
    {
      group = _selectable._type;
    }
  }
  public void SetTypeGroup(SelectableType type, int count)
  {
      _name.Text = $"{type} ({count})";
      _button.Text = "View";
  }

  public void SetSubTypeGroup(int subType, int count)
  {
      _name.Text = $"Subtype {subType} ({count})";
      _button.Text = "View All";
  }
  public override void Tick()
  {
   // _name.Text = _selectable.Name;

  }
}

