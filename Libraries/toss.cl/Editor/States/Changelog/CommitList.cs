using Changelog.Elements;
using Editor;
using Sandbox;

namespace Changelog.States.Changelog;

public sealed class CommitList( Widget parent ) : ListView<CommitEntry>( parent )
{
	protected override void OnPaint( CommitEntry entry, VirtualWidget widget )
	{
		if ( !entry.IsPublished )
		{
			widget.PaintBackground( Theme.WidgetBackground, Theme.ControlRadius );
			Paint.SetPen( Paint.HasSelected || Paint.HasPressed ? Theme.White : Theme.White.Darken( 0.2f ) );
		}
		else
		{
			Paint.SetPen( Theme.White.Darken( 0.5f ) );
		}

		Paint.ClearBrush();
		Paint.SetDefaultFont();

		string[] columns = widget.Rect.Width switch
		{
			> 1000 => [entry.Name, entry.Date, entry.Hash, entry.Author],
			> 700 => [entry.Name, entry.Date, entry.Author],
			> 500 => [entry.Name, entry.Date],
			_ => [entry.Name]
		};

		DrawColumns( widget, columns );

		entry.DrawIcon( widget.Rect.Shrink( 4 ) );
	}
	
	private void DrawColumns( VirtualWidget item, string[] columns )
	{
		var colWidth = item.Rect.Width / columns.Length;
		for ( int i = 0; i < columns.Length; i++ ) {
			var text = columns[i];
			var scaledWidth = ( i == 0 ) ? 2 * colWidth : colWidth;

			var textRect = item.Rect.Shrink( 32, 0, 0, 0 );
			textRect.Left += i * scaledWidth;
			textRect.Right = ( i + 1 ) * scaledWidth;
			
			Paint.DrawText( textRect, text,
				( i == 0 ? TextFlag.LeftCenter : TextFlag.RightCenter ) | TextFlag.SingleLine );
		}
	}
}
