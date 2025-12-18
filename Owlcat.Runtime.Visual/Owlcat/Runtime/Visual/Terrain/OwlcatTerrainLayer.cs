using UnityEngine;

namespace Owlcat.Runtime.Visual.Terrain;

public struct OwlcatTerrainLayer
{
	private readonly TerrainLayer m_TerrainLayer;

	public float Rotation;

	public float Emission;

	public float StampingWeight;

	public float Metallic;

	public float Roughness;

	public float Tiling;

	public OwlcatTerrainLayer(TerrainLayer terrainLayer)
	{
		m_TerrainLayer = terrainLayer;
		Color specular = terrainLayer.specular;
		Rotation = specular.r;
		Emission = specular.g;
		StampingWeight = specular.b;
		Metallic = terrainLayer.metallic;
		Roughness = 1f - terrainLayer.smoothness;
		Tiling = terrainLayer.tileSize.x;
	}

	public void Apply()
	{
		Color specular = m_TerrainLayer.specular;
		specular.r = Rotation;
		specular.g = Emission;
		specular.b = StampingWeight;
		Vector2 tileSize = m_TerrainLayer.tileSize;
		tileSize.x = Tiling;
		m_TerrainLayer.specular = specular;
		m_TerrainLayer.metallic = Metallic;
		m_TerrainLayer.smoothness = 1f - Roughness;
		m_TerrainLayer.tileSize = tileSize;
	}
}
