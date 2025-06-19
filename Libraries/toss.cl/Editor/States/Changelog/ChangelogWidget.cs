using Editor;

namespace Changelog.States.Changelog;

public sealed class ChangelogWidget : Widget
{
	public CommitListContainer CommitListContainer { get; private set; }
	public ChangelogPreview ChangelogPreview { get; private set; }
	
	public ChangelogWidget( Widget parent ) : base( parent )
	{
		Layout = Layout.Row();
		Layout.Margin = 0;
		Layout.Spacing = 4;

		MinimumSize = new Vector2( 350, 200 );

		CommitListContainer = new CommitListContainer( this );
		ChangelogPreview = new ChangelogPreview( this );

		var splitter = new Splitter( this ) { IsHorizontal = true };
		splitter.AddWidget( CommitListContainer );
		splitter.SetStretch( 1, 2 );
		splitter.AddWidget( ChangelogPreview );
		splitter.SetStretch( 0, 1 );

		Layout.Add( splitter );
	}
}
