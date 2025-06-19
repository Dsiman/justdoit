using System.Reflection;
using Changelog.Elements;
using Changelog.Git;
using Editor;
using Sandbox;

namespace Changelog.States.NewCommit;

public sealed class FileEntry : Widget
{
	public new string Name { get; set; }
	public ChangeType Type { get; set; }
	public bool IsStaged { get; set; }
	
	private BoolControlWidget Checkbox { get; }

	private Color Tint = Theme.WidgetBackground;

	public FileEntry( string name, ChangeType type = ChangeType.Unknown )
	{
		Name = name;
		Type = type;

		Layout = Layout.Row();

		HorizontalSizeMode = SizeMode.Flexible;
		
		// typelibrary isn't working for individual enum values so here's some old-fashioned reflection instead!
		// https://stackoverflow.com/a/9276348
		var member = Type.GetType().GetMember( Type.ToString() )[0];
		var icon = member.GetCustomAttribute<IconAttribute>()?.Value;
		var tint = member.GetCustomAttribute<TintAttribute>()?.Tint;
		if ( tint.HasValue )
			Tint = Theme.GetTint( tint.Value );

		Checkbox = new BoolControlWidget( this.GetSerialized().GetProperty( nameof( IsStaged ) ) );
		Checkbox.Parent = this;
		Checkbox.Tint = Tint;
		
		var iconLabel = new IconLabel( icon, this );
		iconLabel.IconSize = 16;
		
		var label = new Label( name, this );

		Layout.Add( Checkbox );
		Layout.Add( iconLabel );
		Layout.Add( label, 1 );
	}

	protected override void OnMouseEnter()
		=> SetStyles( $"background-color: {Tint.Desaturate( 0.6f ).Darken( 0.7f ).Hex}" );

	protected override void OnMouseLeave()
		=> SetStyles( "" );

	protected override void OnMousePress( MouseEvent _ )
		=> IsStaged = !IsStaged;
}
