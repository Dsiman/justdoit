using Sandbox;

namespace MapGen;

[Title("Terrain Generator")]
public sealed class TerrainComponent : Component
{
	[Property] public int Width { get; set; } = 128;
	[Property] public int Depth { get; set; } = 128;
	[Property] public float Zoom { get; set; } = 48f;
	[Property] public float MaxHeight { get; set; } = 20f;
	[Property] public Material OverrideMaterial { get; set; }

	[Property] public bool ForceGenerate { get; set; }

	private ModelRenderer _renderer;

	protected override void OnStart()
	{
		Log.Info($"[TerrainComponent] Starting terrain generation...");

		var seed = SeedGenerator.Instance.GenerateSeed(new MapSeedConfig
		{
			Width = Width,
			Depth = Depth,
			Zoom = Zoom,
			MaxHeight = MaxHeight,
			ExtraData = ""
		});

		Log.Info($"[TerrainComponent] Seed generated: {seed}");

		Model model;
		TerrainMeshGenerator.TerrainTextures textures;

		if (ForceGenerate)
		{
			Log.Warning("[TerrainComponent] ForceGenerate is enabled — bypassing cache.");
			model = TerrainMeshGenerator.GenerateModel(seed, out textures);
			ForceGenerate = false;
		}
		else
		{
			model = TerrainCache.GetOrGenerate(seed, out textures);
		}

		if (model == null)
		{
			Log.Error("[TerrainComponent] Model is null after generation!");
			return;
		}

		_renderer = Components.GetOrCreate<ModelRenderer>();
		_renderer.Model = model;
		_renderer.MaterialOverride = OverrideMaterial;
		
		new Sandbox.SceneCullingBox( Scene.SceneWorld, GameObject.Transform.World, GameObject.WorldScale, SceneCullingBox.CullMode.Outside );

		if ( OverrideMaterial != null )
		{
			Log.Info( "[TerrainComponent] Applying texture maps to material." );
			//OverrideMaterial.SetTexture("normal_map", textures.NormalMap);
			//OverrideMaterial.SetTexture("biome_map", textures.BiomeMap);
			//OverrideMaterial.SetTexture("erosion_map", textures.ErosionMap);
		}
		else
		{
			Log.Warning( "[TerrainComponent] OverrideMaterial not set in inspector!" );
		}

		Log.Info($"[TerrainComponent] Terrain model '{model.Name}' assigned to GameObject.");
	}
}
