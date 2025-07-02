using Sandbox;
using System;
using System.Linq;

[Title( "Island Map Generator" )]
[Category( "Tools" )]
public sealed class IslandMapGenerator : Component
{
	[Property, Range( 8, 256 )] public int GridSize { get; set; } = 64;
	[Property, Range( 16, 128 )] public float TileSpacing { get; set; } = 32f;
	[Property, Range( 1f, 100f )] public float NoiseScale { get; set; } = 20f;
	[Property, Range( 1f, 50f )] public float HeightMultiplier { get; set; } = 10f;
	[Property] public GameObject TilePrefab { get; set; }
	[Property, Range( 0f, 1f )] public float WaterLevel { get; set; } = 0.3f;
	[Property] public bool RegenerateOnPlay { get; set; } = true;

	protected override void OnStart()
	{
		if ( RegenerateOnPlay )
			GenerateMap();
	}

	public void GenerateMap()
	{
		if ( TilePrefab == null )
		{
			Log.Warning( "TilePrefab not set!" );
			return;
		}

		// Delete existing children
		foreach ( var child in GameObject.Children.ToList() )
			child.Destroy();

		float center = GridSize / 2f;
		float radius = GridSize / 2f;

		for ( int x = 0; x < GridSize; x++ )
		{
			for ( int y = 0; y < GridSize; y++ )
			{
				Vector2 pos = new( x, y );
				float dist = Vector2.Distance( pos, new Vector2( center, center ) ) / radius;
				float falloff = 1f - Math.Clamp( dist, 0f, 1f );

				// Simple noise using Sin + Cos (as fallback)
				float nx = (x + 1000f) / NoiseScale;
				float ny = (y + 1000f) / NoiseScale;
				float noise = (float)(Math.Sin( nx ) * Math.Cos( ny ));

				// Simple wavy river shape
				float river = (float)Math.Abs( Math.Sin( x * 0.1f + 1.5f ) ) - 0.3f;
				river = Math.Clamp( river, 0f, 1f );

				float height = noise * falloff * river * HeightMultiplier;

				var tile = TilePrefab.Clone();
				tile.Name = $"Tile_{x}_{y}";
				tile.WorldPosition = new Vector3( x * TileSpacing, y * TileSpacing, height * TileSpacing );
				tile.SetParent( GameObject );
			}
		}
	}
}
