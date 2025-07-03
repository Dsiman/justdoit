using System;
using System.Text;

namespace MapGen;

public sealed class SeedGenerator
{
	private static readonly Lazy<SeedGenerator> _lazy = new(() => new SeedGenerator());
	public static SeedGenerator Instance => _lazy.Value;

	private SeedGenerator() { }

	public string GenerateSeed(MapSeedConfig config)
	{
		var sb = new StringBuilder();
		sb.Append(config.Width.ToString("D4"));         // 4 digits
		sb.Append(config.Depth.ToString("D4"));         // 4 digits
		sb.Append(config.Zoom.ToString("0000.00"));     // 6 chars
		sb.Append(config.MaxHeight.ToString("0000.00")); // 6 chars

		if (config.ExtraData != null)
			sb.Append(config.ExtraData);

		return sb.ToString();
	}
}

public class MapSeedConfig
{
	public int Width;
	public int Depth;
	public float Zoom;
	public float MaxHeight;
	public string ExtraData;
}