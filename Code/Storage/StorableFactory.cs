using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sandbox;

public static class StorableFactory
{
	/// <summary>
	/// Converts a StoredItem into a JSON string.
	/// </summary>
	public static string SerializeStoredItem( StoredItem item )
	{
		if ( item == null ) return null;

		var data = new
		{
			PreviewModelPath = item.Previewmodel?.ResourcePath,
			TexturePath = item.texture?.ResourcePath,
			ItemShapeType = item.itemShape.ShapeType,
			CustomGrid = item.itemShape.ShapeType == ItemShapeType.Custom
				? ConvertGridToList( item.itemShape.Grid )
				: null,
			GameObject = SerializeGameObject( item.itemToStore )
		};

		return JsonSerializer.Serialize( data );
	}

	/// <summary>
	/// Recreates a StoredItem from its JSON string.
	/// </summary>
	public static StoredItem DeserializeStoredItem( string json )
	{
		if ( string.IsNullOrWhiteSpace( json ) )
			return null;

		try
		{
			var parsed = JsonNode.Parse( json )?.AsObject();
			if ( parsed == null ) return null;

			// Deserialize metadata
			var previewPath = parsed["PreviewModelPath"]?.GetValue<string>();
			var texturePath = parsed["TexturePath"]?.GetValue<string>();
			var shapeType = parsed["ItemShapeType"]?.GetValue<ItemShapeType>() ?? ItemShapeType.Square_1x1;

			// Handle custom grid shape
			bool[,] customGrid = null;
			if ( shapeType == ItemShapeType.Custom )
			{
				var gridList = parsed["CustomGrid"]?.AsArray();
				if ( gridList != null )
				{
					customGrid = ConvertListToGrid( gridList );
				}
			}

			// Deserialize game object
			var goJson = parsed["GameObject"]?.GetValue<string>();
			var gameObject = DeserializeGameObject( goJson );

			// Reconstruct stored item
			var item = new StoredItem
			{
				Previewmodel = !string.IsNullOrEmpty( previewPath ) ? Model.Load( previewPath ) : null,
				texture = !string.IsNullOrEmpty( texturePath ) ? Texture.Load( texturePath ) : null,
				itemShape = customGrid != null
					? new ItemShape( ItemShapeType.Custom ) { Grid = customGrid }
					: new ItemShape( shapeType ),
				itemToStore = gameObject
			};

			return item;
		}
		catch ( Exception e )
		{
			Log.Error( $"StoredItem deserialization failed: {e.Message}" );
			return null;
		}
	}

	// Helper: Convert 2D grid to JSON-serializable list
	private static JsonArray ConvertGridToList( bool[,] grid )
	{
		var rows = grid.GetLength( 0 );
		var cols = grid.GetLength( 1 );
		var list = new JsonArray();

		for ( int i = 0; i < rows; i++ )
		{
			var row = new JsonArray();
			for ( int j = 0; j < cols; j++ )
			{
				row.Add( grid[i, j] );
			}
			list.Add( row );
		}
		return list;
	}

	// Helper: Convert list back to 2D grid
	private static bool[,] ConvertListToGrid( JsonArray gridList )
	{
		if ( gridList == null || gridList.Count == 0 )
			return new bool[0, 0];

		var rows = gridList.Count;
		var cols = gridList[0]?.AsArray().Count ?? 0;
		var grid = new bool[rows, cols];

		for ( int i = 0; i < rows; i++ )
		{
			var row = gridList[i]?.AsArray();
			if ( row == null ) continue;

			for ( int j = 0; j < Math.Min( cols, row.Count ); j++ )
			{
				grid[i, j] = row[j]?.GetValue<bool>() ?? false;
			}
		}
		return grid;
	}

	// Keep GameObject serialization for internal use
	private static string SerializeGameObject( GameObject item )
	{
		return item?.Serialize()?.ToJsonString();
	}

	private static GameObject DeserializeGameObject( string json )
	{
		if ( string.IsNullOrWhiteSpace( json ) ) return null;

		try
		{
			JsonObject parsed = JsonNode.Parse( json )?.AsObject();
			return parsed == null ? null : Json.Deserialize<GameObject>( parsed.ToJsonString() );
		}
		catch ( Exception e )
		{
			Log.Error( $"GameObject deserialization failed: {e.Message}" );
			return null;
		}
	}
}