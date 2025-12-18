using Owlcat.Runtime.Visual.VirtualTerrain;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping;

[ExecuteAlways]
public class OwlcatTerrainStampingZone : MonoBehaviour
{
	private static class ShaderProperties
	{
		public static readonly int _DetailNormals = Shader.PropertyToID("_DetailNormals");

		public static readonly int _DetailNormals_ST = Shader.PropertyToID("_DetailNormals_ST");

		public static readonly int _DetailNormalsWeight = Shader.PropertyToID("_DetailNormalsWeight");

		public static readonly int _DetailNormalsStrength = Shader.PropertyToID("_DetailNormalsStrength");

		public static readonly int _DetailNormalsDepthFade = Shader.PropertyToID("_DetailNormalsDepthFade");

		public static readonly int _AlphaBlendFactor = Shader.PropertyToID("_AlphaBlendFactor");

		public static readonly int _AlphaGain = Shader.PropertyToID("_AlphaGain");

		public static readonly int _TerrainUVScaleOffset = Shader.PropertyToID("_TerrainUVScaleOffset");
	}

	[SerializeField]
	private MeshRenderer m_MeshRenderer;

	[SerializeField]
	private OwlcatVirtualTerrain m_OwlcatTerrain;

	[Header("Details")]
	[SerializeField]
	private Texture2D m_DetailNormals;

	[SerializeField]
	private Vector2 m_DetailNormalsScale = Vector2.one;

	[SerializeField]
	private Vector2 m_DetailNormalsOffset = Vector2.zero;

	[SerializeField]
	[Range(0f, 5f)]
	private float m_DetailNormalsWeight = 1f;

	[SerializeField]
	[Range(0f, 50f)]
	private float m_DetailNormalsStrength = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_DetailNormalsDepthFade = 0.75f;

	private MaterialPropertyBlock m_PropertyBlock;

	private void Awake()
	{
		if (Application.isPlaying)
		{
			base.transform.hideFlags = HideFlags.NotEditable;
			base.hideFlags = HideFlags.NotEditable;
		}
	}

	private void OnEnable()
	{
		TerrainStampingZoneContainer.Remove(this);
		if (!(m_OwlcatTerrain == null))
		{
			m_OwlcatTerrain.TerrainSplatMaterialPropertyBlockUpdated -= OnAppliedMaterialProperties;
			m_OwlcatTerrain.TerrainSplatMaterialPropertyBlockUpdated += OnAppliedMaterialProperties;
			ApplyProperties();
			TerrainStampingZoneContainer.Add(this);
		}
	}

	private void OnDisable()
	{
		TerrainStampingZoneContainer.Remove(this);
		if ((object)m_OwlcatTerrain != null)
		{
			m_OwlcatTerrain.TerrainSplatMaterialPropertyBlockUpdated -= OnAppliedMaterialProperties;
		}
	}

	private void OnValidate()
	{
		TerrainStampingZoneContainer.Remove(this);
		if (m_OwlcatTerrain != null)
		{
			m_OwlcatTerrain.TerrainSplatMaterialPropertyBlockUpdated -= OnAppliedMaterialProperties;
			m_OwlcatTerrain.TerrainSplatMaterialPropertyBlockUpdated += OnAppliedMaterialProperties;
			ApplyProperties();
			TerrainStampingZoneContainer.Add(this);
		}
	}

	private void OnAppliedMaterialProperties(OwlcatVirtualTerrain sender, MaterialPropertyBlock properties)
	{
		ApplyProperties();
	}

	private void ApplyProperties()
	{
		if (m_OwlcatTerrain.TryGetComponent<UnityEngine.Terrain>(out var component))
		{
			if (m_PropertyBlock == null)
			{
				m_PropertyBlock = new MaterialPropertyBlock();
			}
			component.GetSplatMaterialPropertyBlock(m_PropertyBlock);
			GetTerrainMinMax(component, out var terrainMin, out var terrainMax);
			float4 @float = math.float4(1f / (terrainMax - terrainMin), -terrainMin / (terrainMax - terrainMin));
			m_PropertyBlock.SetVector(ShaderProperties._TerrainUVScaleOffset, @float);
			Material materialTemplate = component.materialTemplate;
			if (materialTemplate != null)
			{
				m_PropertyBlock.SetFloat(ShaderProperties._AlphaBlendFactor, materialTemplate.GetFloat(ShaderProperties._AlphaBlendFactor));
				m_PropertyBlock.SetFloat(ShaderProperties._AlphaGain, materialTemplate.GetFloat(ShaderProperties._AlphaGain));
			}
			m_PropertyBlock.SetTexture(ShaderProperties._DetailNormals, (m_DetailNormals != null) ? m_DetailNormals : Texture2D.normalTexture);
			m_PropertyBlock.SetVector(ShaderProperties._DetailNormals_ST, new Vector4(m_DetailNormalsScale.x, m_DetailNormalsScale.y, m_DetailNormalsOffset.x, m_DetailNormalsOffset.y));
			m_PropertyBlock.SetFloat(ShaderProperties._DetailNormalsWeight, m_DetailNormalsWeight);
			m_PropertyBlock.SetFloat(ShaderProperties._DetailNormalsStrength, m_DetailNormalsStrength);
			m_PropertyBlock.SetFloat(ShaderProperties._DetailNormalsDepthFade, 1f - m_DetailNormalsDepthFade);
			m_MeshRenderer.SetPropertyBlock(m_PropertyBlock);
		}
	}

	private static void GetTerrainMinMax(UnityEngine.Terrain terrain, out float2 terrainMin, out float2 terrainMax)
	{
		Transform obj = terrain.transform;
		float3 @float = obj.position;
		float3 float2 = obj.lossyScale;
		float3 float3 = terrain.terrainData.size;
		terrainMin = @float.xz;
		terrainMax = terrainMin + float3.xz * float2.xz;
	}

	public void GetWorldAABB(out float3 center, out float3 extents)
	{
		Transform transform = base.transform;
		extents = math.max(float3.zero, (float3)transform.localScale * 0.5f);
		center = transform.position;
	}
}
