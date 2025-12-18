using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Owlcat.Runtime.Visual.GPUDrivenBRG.BakedLighting;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Terrain;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(UnityEngine.Terrain))]
public class OwlcatTerrain : OwlcatTerrainBase
{
	public enum TerrainSizes
	{
		Fine_6,
		Diminutive_12,
		Tiny_25,
		Small_50,
		Small_75,
		Medium_100,
		Medium_150,
		Large_200,
		Large_250,
		Large_300,
		Huge_350,
		Huge_400
	}

	public enum Tex2DArrayResolution
	{
		x128 = 0x80,
		x256 = 0x100,
		x512 = 0x200,
		x1024 = 0x400,
		x2048 = 0x800
	}

	public struct State
	{
		public static readonly int WeightsAlignment = Marshal.SizeOf<Vector4>() / 4;

		public Vector4[] LayerDataMasksScale;

		public Vector4[] LayerDataUvMatrix;

		public Vector4[] LayerDataParams;

		public Vector4[] LayerStampingWeights;

		public Texture2DArray BakedDiffuse;

		public Texture2DArray BakedNormalMap;

		public Texture2DArray BakedMaskMap;

		public Texture2DArray BakedSplatMap;

		public void SetStampingWeightAt(int index, float weight)
		{
			LayerStampingWeights[index / 4][index % 4] = weight;
		}
	}

	private static readonly Vector4[] s_VectorArray256 = new Vector4[256];

	[HideInInspector]
	public TerrainSizes TerrainSize = TerrainSizes.Small_50;

	[SerializeField]
	private Vector4[] m_LayerDataMasksScale = Array.Empty<Vector4>();

	[SerializeField]
	private Vector4[] m_LayerDataUvMatrix = Array.Empty<Vector4>();

	[SerializeField]
	private Vector4[] m_LayerDataParams = Array.Empty<Vector4>();

	[SerializeField]
	private Vector4[] m_LayerDataStampingWeights = Array.Empty<Vector4>();

	[SerializeField]
	private Texture2DArray m_Diffuse;

	[SerializeField]
	private Texture2DArray m_Normal;

	[SerializeField]
	private Texture2DArray m_Masks;

	[SerializeField]
	private Texture2DArray m_SplatArray;

	public Tex2DArrayResolution TexturesResolution = Tex2DArrayResolution.x256;

	private MaterialPropertyBlock m_MaterialPropertyBlock;

	public override Texture2DArray SplatArray => m_SplatArray;

	public Texture2DArray DiffuseArray => m_Diffuse;

	public Texture2DArray NormalArray => m_Normal;

	public Texture2DArray MasksArray => m_Masks;

	[UsedImplicitly]
	private void OnEnable()
	{
		if (!Application.isEditor && TryGetComponent<UnityEngine.Terrain>(out var component))
		{
			Texture2D[] alphamapTextures = component.terrainData.alphamapTextures;
			for (int i = 0; i < alphamapTextures.Length; i++)
			{
				UnityEngine.Object.DestroyImmediate(alphamapTextures[i]);
			}
		}
		ApplyMaterialProperties();
	}

	public State GetState()
	{
		State result = default(State);
		result.LayerDataMasksScale = m_LayerDataMasksScale;
		result.LayerDataUvMatrix = m_LayerDataUvMatrix;
		result.LayerDataParams = m_LayerDataParams;
		result.LayerStampingWeights = m_LayerDataStampingWeights;
		result.BakedDiffuse = m_Diffuse;
		result.BakedNormalMap = m_Normal;
		result.BakedMaskMap = m_Masks;
		result.BakedSplatMap = m_SplatArray;
		return result;
	}

	public void SetState(in State state)
	{
		m_LayerDataMasksScale = state.LayerDataMasksScale;
		m_LayerDataUvMatrix = state.LayerDataUvMatrix;
		m_LayerDataParams = state.LayerDataParams;
		m_LayerDataStampingWeights = state.LayerStampingWeights;
		m_Diffuse = state.BakedDiffuse;
		m_Normal = state.BakedNormalMap;
		m_Masks = state.BakedMaskMap;
		m_SplatArray = state.BakedSplatMap;
		ApplyMaterialProperties();
	}

	public void ApplyMaterialProperties()
	{
		if (TryGetComponent<UnityEngine.Terrain>(out var component))
		{
			if (m_MaterialPropertyBlock == null)
			{
				m_MaterialPropertyBlock = new MaterialPropertyBlock();
			}
			try
			{
				PopulateMaterialProperties(m_MaterialPropertyBlock);
				component.SetSplatMaterialPropertyBlock(m_MaterialPropertyBlock);
			}
			finally
			{
				m_MaterialPropertyBlock.Clear();
			}
			GPUDrivenLightmapReference.RequestLightmapUnpack(component.gameObject.scene, component.lightmapIndex);
		}
	}

	public void PopulateMaterialProperties(MaterialPropertyBlock properties)
	{
		if (properties != null && TryGetComponent<UnityEngine.Terrain>(out var component))
		{
			TerrainData terrainData = component.terrainData;
			if (!(terrainData == null))
			{
				Vector4[] layerDataMasksScale = m_LayerDataMasksScale;
				Vector4[] layerDataUvMatrix = m_LayerDataUvMatrix;
				Vector4[] layerDataParams = m_LayerDataParams;
				Vector4[] layerDataStampingWeights = m_LayerDataStampingWeights;
				Texture2DArray diffuse = m_Diffuse;
				Texture2DArray normal = m_Normal;
				Texture2DArray masks = m_Masks;
				SetVectorArray(OwlcatTerrainShader.TerrainLayerMasksScale, layerDataMasksScale);
				SetVectorArray(OwlcatTerrainShader.TerrainLayerUvMatrix, layerDataUvMatrix);
				SetVectorArray(OwlcatTerrainShader.TerrainLayerParams, layerDataParams);
				SetVectorArray(OwlcatTerrainShader.TerrainLayerStampingWeights, layerDataStampingWeights);
				SetTexture(OwlcatTerrainShader.DiffuseArray, diffuse);
				SetTexture(OwlcatTerrainShader.NormalArray, normal);
				SetTexture(OwlcatTerrainShader.MasksArray, masks);
				SetTexture(OwlcatTerrainShader.SplatArray, m_SplatArray);
				properties.SetInt(OwlcatTerrainShader.ControlTexturesCount, (m_SplatArray != null) ? m_SplatArray.depth : 0);
				properties.SetFloat(OwlcatTerrainShader.TerrainMaxHeight, 1f / terrainData.size.x);
			}
		}
		void SetTexture(int propertyId, Texture value)
		{
			if (value != null)
			{
				properties.SetTexture(propertyId, value);
			}
		}
		void SetVectorArray(int propertyId, Vector4[] value)
		{
			if (value.Length != 0)
			{
				Array.Copy(value, s_VectorArray256, value.Length);
				properties.SetVectorArray(propertyId, s_VectorArray256);
			}
		}
	}
}
