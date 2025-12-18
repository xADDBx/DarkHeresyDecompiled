using Owlcat.Runtime.Visual.VirtualTerrain;
using UnityEngine;

namespace Owlcat.Runtime.Visual.ExtendedTerrainPainting;

[ExecuteAlways]
public sealed class OwlcatExtendedTerrainPainting : MonoBehaviour
{
	public enum MapResolution
	{
		_16 = 0x10,
		_32 = 0x20,
		_64 = 0x40,
		_128 = 0x80,
		_256 = 0x100,
		_512 = 0x200
	}

	private static class ShaderIDs
	{
		public static readonly int _TintWetnessMap = Shader.PropertyToID("_TintWetnessMap");

		public static readonly int _WetnessAlbedoDarkenFactor = Shader.PropertyToID("_WetnessAlbedoDarkenFactor");
	}

	public const RenderTextureFormat kTintFormat = RenderTextureFormat.ARGB32;

	[SerializeField]
	[HideInInspector]
	private OwlcatVirtualTerrain m_OwlcatTerrain;

	[SerializeField]
	[HideInInspector]
	private Texture2D m_TintWetnessMap;

	[SerializeField]
	private MapResolution m_TintWetnessMapResolution = MapResolution._256;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_WetnessAlbedoDarkenFactor = 0.5f;

	private void OnEnable()
	{
		if (m_OwlcatTerrain != null)
		{
			m_OwlcatTerrain.TerrainSplatMaterialPropertyBlockUpdating -= OnMaterialPropertyBlockUpdating;
			m_OwlcatTerrain.TerrainSplatMaterialPropertyBlockUpdating += OnMaterialPropertyBlockUpdating;
			m_OwlcatTerrain.PopulatingMaterialProperties -= OnPopulatingMaterialProperties;
			m_OwlcatTerrain.PopulatingMaterialProperties += OnPopulatingMaterialProperties;
		}
	}

	private void OnDisable()
	{
		if (m_OwlcatTerrain != null)
		{
			m_OwlcatTerrain.TerrainSplatMaterialPropertyBlockUpdating -= OnMaterialPropertyBlockUpdating;
			m_OwlcatTerrain.PopulatingMaterialProperties -= OnPopulatingMaterialProperties;
		}
	}

	private void OnPopulatingMaterialProperties(Material material)
	{
		material.SetTexture(ShaderIDs._TintWetnessMap, (m_TintWetnessMap != null) ? m_TintWetnessMap : Texture2D.whiteTexture);
		material.SetFloat(ShaderIDs._WetnessAlbedoDarkenFactor, m_WetnessAlbedoDarkenFactor);
	}

	public bool TryGetTintWetnessMap(out Texture2D tintMap)
	{
		tintMap = m_TintWetnessMap;
		return tintMap != null;
	}

	private void OnMaterialPropertyBlockUpdating(OwlcatVirtualTerrain sender, MaterialPropertyBlock properties)
	{
		SetProperties(properties);
	}

	private void SetProperties(MaterialPropertyBlock properties)
	{
		properties.SetTexture(ShaderIDs._TintWetnessMap, (m_TintWetnessMap != null) ? m_TintWetnessMap : Texture2D.whiteTexture);
		properties.SetFloat(ShaderIDs._WetnessAlbedoDarkenFactor, m_WetnessAlbedoDarkenFactor);
	}

	public void TryInitializeTintWetnessMap(TerrainData terrainData)
	{
	}
}
