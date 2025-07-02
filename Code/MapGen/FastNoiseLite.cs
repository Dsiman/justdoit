public class FastNoiseLite
{
	public enum NoiseType { OpenSimplex2 }

	public void SetNoiseType( NoiseType type ) { }
	public void SetFrequency( float freq ) { }

	// Simple placeholder noise function
	public float GetNoise( float x, float y ) => (float)(System.Math.Sin( x ) * System.Math.Cos( y ));
}