using System;
using System.Collections.Generic;
using Sandbox;

namespace MapGen;

/// <summary>
/// Simplifies meshes by removing triangles that are very flat (similar normals).
/// </summary>
public sealed class MeshOptimizer
{
	private static readonly Lazy<MeshOptimizer> _lazy = new(() => new MeshOptimizer());
	public static MeshOptimizer Instance => _lazy.Value;

	/// <summary>
	/// Dot product similarity threshold. 1.0 = identical normals, 0.0 = perpendicular
	/// </summary>
	private const float NormalThreshold = 0.999f; // Tightened threshold

	/// <summary>
	/// How steep must a triangle be to be "interesting"
	/// </summary>
	private const float MinSlopeZ = 0.99f; // Don't remove even gentle slopes

	private MeshOptimizer() { }

	public void SimplifyMesh(ref List<Vertex> verts, ref List<int> indices, int width, int depth)
	{
		Log.Info("[MeshOptimizer] Simplifying mesh...");

		var newIndices = new List<int>();
		int removed = 0;

		for (int i = 0; i < indices.Count; i += 3)
		{
			var i0 = indices[i];
			var i1 = indices[i + 1];
			var i2 = indices[i + 2];

			var n0 = verts[i0].Normal;
			var n1 = verts[i1].Normal;
			var n2 = verts[i2].Normal;

			// Check if normals are nearly identical
			float dot1 = Vector3.Dot(n0, n1);
			float dot2 = Vector3.Dot(n1, n2);
			float dot3 = Vector3.Dot(n2, n0);

			bool allSimilar = dot1 > NormalThreshold && dot2 > NormalThreshold && dot3 > NormalThreshold;

			// Also check: are they nearly flat (facing up)?
			float avgZ = (n0.z + n1.z + n2.z) / 3f;

			bool isFlat = avgZ > MinSlopeZ;

			if (allSimilar && isFlat)
			{
				removed++;
				continue;
			}

			newIndices.Add(i0);
			newIndices.Add(i1);
			newIndices.Add(i2);
		}

		Log.Info($"[MeshOptimizer] Removed {removed} flat triangles out of {indices.Count / 3}");
		indices = newIndices;
	}
}
