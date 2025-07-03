using System.Collections.Generic;
using Sandbox;

namespace MapGen;

public static class TerrainCache
{
	private static Dictionary<string, Model> _cached = new();
	private static Dictionary<string, TerrainMeshGenerator.TerrainTextures> _textureCache = new();

	public static Model GetOrGenerate(string seed, out TerrainMeshGenerator.TerrainTextures textures)
	{
		if (_cached.TryGetValue(seed, out var model))
		{
			Log.Info($"[TerrainCache] Using cached model for seed '{seed}'");
			textures = _textureCache[seed];
			return model;
		}

		Log.Info($"[TerrainCache] Generating new model for seed '{seed}'...");
		model = TerrainMeshGenerator.GenerateModel(seed, out textures);

		if (model == null)
		{
			Log.Error($"[TerrainCache] Model generation failed for seed '{seed}'");
			return null;
		}

		_cached[seed] = model;
		_textureCache[seed] = textures;

		Log.Info($"[TerrainCache] Model generated and cached successfully for seed '{seed}'");
		return model;
	}
}
