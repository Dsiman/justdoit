using Editor;
using Sandbox;

namespace Changelog.States.Changelog;

public sealed class CommitEntry : IAssetListEntry
{
	public string Name { get; }
	public string Author { get; }
	public string Date { get; }
	public string Hash { get; }

	public bool IsHidden { get; set; } = false;
	public bool IsPublished { get; set; } = false;
	public bool IsLatest { get; set; } = false;

	public CommitEntry( string name, string hash, string date, string author = "???" )
	{
		Name = name;
		Author = author;
		Date = date;
		Hash = hash;
	}

	private string Icon => IsHidden
		? "visibility_off"
		: IsLatest
			? "published_with_changes"
			: "";

	public void DrawIcon( Rect rect )
	{
		Paint.BilinearFiltering = true;

		Paint.ClearPen();

		Paint.SetPen( Theme.ControlText.WithAlpha( 0.75f ) );
		Paint.DrawIcon( rect, Icon, rect.Height, TextFlag.LeftCenter );

		Paint.BilinearFiltering = false;
	}
}
