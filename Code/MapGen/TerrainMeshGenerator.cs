using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.Utility;

namespace MapGen;

public static class TerrainMeshGenerator
{
	public struct TerrainTextures
	{
		public Texture NormalMap;
		public Texture BiomeMap;
		public Texture ErosionMap;
	}

	public static Model GenerateModel(string seed, out TerrainTextures textures)
	{
		var (width, depth, zoom, maxHeight) = ParseSeed(seed);
		Log.Info($"[TerrainMeshGenerator] Generating mesh (width={width}, depth={depth}, zoom={zoom}, maxHeight={maxHeight})");

		var vertexGrid = new Vertex[width, depth];
		var indices = new List<int>();
		var heights = new float[width, depth];

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < depth; y++)
			{
				float noise = Noise.Fbm(4, x / zoom, y / zoom, 0f);
				float height = noise * maxHeight;
				heights[x, y] = height;

				var pos = new Vector3(x, y, height);
				vertexGrid[x, y] = new Vertex(pos, Vector3.Zero, new Vector4(1, 0, 0, 1), Vector4.Zero);
			}
		}

		Log.Info("[TerrainMeshGenerator] Calculating normals...");

		for (int x = 1; x < width - 1; x++)
		{
			for (int y = 1; y < depth - 1; y++)
			{
				float hL = heights[x - 1, y];
				float hR = heights[x + 1, y];
				float hD = heights[x, y - 1];
				float hU = heights[x, y + 1];

				Vector3 normal = new Vector3(hL - hR, hD - hU, 2f).Normal;
				vertexGrid[x, y].Normal = normal;
			}
		}

		Log.Info("[TerrainMeshGenerator] Creating index buffer...");
		for (int x = 0; x < width - 1; x++)
		{
			for (int y = 0; y < depth - 1; y++)
			{
				int i = x + y * width;
				indices.Add(i);
				indices.Add(i + 1);
				indices.Add(i + width);

				indices.Add(i + 1);
				indices.Add(i + width + 1);
				indices.Add(i + width);
			}
		}

		var flatVerts = new List<Vertex>(width * depth);
		for (int y = 0; y < depth; y++)
			for (int x = 0; x < width; x++)
				flatVerts.Add(vertexGrid[x, y]);

		//Log.Info("[TerrainMeshGenerator] Running optimization pass...");
		//MeshOptimizer.Instance.SimplifyMesh(ref flatVerts, ref indices, width, depth);

		var mesh = new Mesh();
		mesh.CreateVertexBuffer(flatVerts.Count, Vertex.Layout, flatVerts);
		mesh.CreateIndexBuffer(indices.Count, indices);

		Log.Info("[TerrainMeshGenerator] Generating textures...");
		textures = TextureGenerator.GenerateMaps(width, depth, heights);

		var model = Model.Builder
			.AddMesh(mesh)
			.WithName($"{seed}.vmdl")
			.Create();

		Log.Info($"[TerrainMeshGenerator] Model generation complete: {model.Name}");
		return model;
	}

	private static (int Width, int Depth, float Zoom, float MaxHeight) ParseSeed(string seed)
	{
		int width = int.Parse(seed.Substring(0, 4));
		int depth = int.Parse(seed.Substring(4, 4));
		float zoom = float.Parse(seed.Substring(8, 6));
		float maxHeight = float.Parse(seed.Substring(14, 6));
		return (width, depth, zoom, maxHeight);
	}
}
