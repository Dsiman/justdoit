using Sandbox;
using System;

namespace MapGen;

public static class TextureGenerator
{
	public static TerrainMeshGenerator.TerrainTextures GenerateMaps(int width, int height, float[,] heights)
	{
		Log.Info("[TextureGenerator] Generating maps...");

		var normalData = new Color[width * height];
		var biomeData = new Color[width * height];
		var erosionData = new Color[width * height];

		for (int x = 1; x < width - 1; x++)
		{
			for (int y = 1; y < height - 1; y++)
			{
				int i = x + y * width;

				float hl = heights[x - 1, y];
				float hr = heights[x + 1, y];
				float hd = heights[x, y - 1];
				float hu = heights[x, y + 1];

				Vector3 normal = new Vector3(hl - hr, hd - hu, 2f).Normal;
				normalData[i] = new Color(normal.x * 0.5f + 0.5f, normal.y * 0.5f + 0.5f, normal.z * 0.5f + 0.5f);

				float h = heights[x, y];
				biomeData[i] = h < 5f ? Color.Blue : h > 15f ? Color.White : Color.Green;

				float erosion = Math.Abs(hl - hr) + Math.Abs(hd - hu);
				erosionData[i] = Color.Lerp(Color.Black, Color.Red, Math.Clamp(erosion, 0, 1));
			}
		}

		Log.Info("[TextureGenerator] Creating GPU textures...");

		return new TerrainMeshGenerator.TerrainTextures
		{
			NormalMap = BuildTexture("NormalMap", width, height, normalData),
			BiomeMap = BuildTexture("BiomeMap", width, height, biomeData),
			ErosionMap = BuildTexture("ErosionMap", width, height, erosionData)
		};
	}

	private static Texture BuildTexture(string name, int width, int height, Color[] pixels)
	{
		byte[] raw = new byte[width * height * 4];
		for (int i = 0; i < pixels.Length; i++)
		{
			var c = pixels[i];
			raw[i * 4 + 0] = (byte)(c.r * 255);
			raw[i * 4 + 1] = (byte)(c.g * 255);
			raw[i * 4 + 2] = (byte)(c.b * 255);
			raw[i * 4 + 3] = (byte)(c.a * 255);
		}

		var tex = Texture.Create(width, height, ImageFormat.RGBA8888)
			.WithName(name)
			.WithData(raw)
			.Finish();

		Log.Info($"[TextureGenerator] Texture '{name}' created ({width}x{height})");
		return tex;
	}
}
