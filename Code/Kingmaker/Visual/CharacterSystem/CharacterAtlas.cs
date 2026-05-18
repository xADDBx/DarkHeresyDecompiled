using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.Runtime.Visual.DxtCompressor;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class CharacterAtlas
{
	public bool Destroyed;

	private List<CharacterTextureDescription> m_SortedTextures;

	private Dictionary<Texture, SortedSet<BodyPartType>> m_TexturesTypesMap;

	private Dictionary<CharacterTextureDescription, Texture> m_PrimaryTextureMap;

	private readonly CharacterAtlasData m_CharacterAtlasDataLocal;

	private readonly Vector2 m_Size;

	private bool m_CompressingNow;

	private RenderTexture m_UncompressedTexture;

	private Texture2D AtlasTexture;

	private readonly Dictionary<Texture, Rect> m_Rects;

	private static RenderTexture m_TempLinearTexture;

	private static RenderTexture m_TempSRGBTexture;

	private static readonly string[] m_DiffuseBlendingKeywords = new string[4] { "DIFFUSE_ADD", "DIFFUSE_MULT", "DIFFUSE_OVERLAY", "DIFFUSE_SCREEN" };

	private readonly Vector2 m_ScaleFactor;

	private Material m_ShadowBakeMaterial;

	private Material m_DiffuseBakeMaterial;

	private Material m_RoughnessLightenBlend;

	public CharacterTextureChannel Channel { get; private set; }

	private Material BakeMaterial { get; } = new Material(Shader.Find("Hidden/CharacterAtlasGenerator"));


	private Material ShadowBakeMaterial => m_ShadowBakeMaterial ?? (m_ShadowBakeMaterial = new Material(Shader.Find("Hidden/CharacterShadowBake")));

	private Material DiffuseBakeMaterial => m_DiffuseBakeMaterial ?? (m_DiffuseBakeMaterial = new Material(Shader.Find("Hidden/CharacterDiffuseBake")));

	private Material RoughnessLightenBlend => m_RoughnessLightenBlend ?? (m_RoughnessLightenBlend = new Material(Shader.Find("Hidden/RoughnessLightenBlend")));

	public void RefreshData()
	{
		m_TexturesTypesMap = new Dictionary<Texture, SortedSet<BodyPartType>>();
		m_PrimaryTextureMap = new Dictionary<CharacterTextureDescription, Texture>();
		m_SortedTextures = new List<CharacterTextureDescription>();
	}

	public CharacterAtlas(CharacterAtlasSize size, CharacterTextureChannel channel, CharacterAtlasData atlasData)
	{
		m_CharacterAtlasDataLocal = atlasData;
		m_Rects = new Dictionary<Texture, Rect>();
		Channel = channel;
		m_Size = new Vector2((float)size.X, (float)size.Y);
		m_ScaleFactor = new Vector2((float)m_CharacterAtlasDataLocal.TargetResolution.X / (float)size.X, (float)m_CharacterAtlasDataLocal.TargetResolution.Y / (float)size.Y);
	}

	public void AddPrimaryTexture(CharacterTextureDescription textureDesc, BodyPartType bodyPartType)
	{
		if (textureDesc == null)
		{
			PFLog.Default.Warning($"TextureDesc is null {bodyPartType}");
			return;
		}
		if (textureDesc.GetSourceTexture() == null)
		{
			PFLog.Default.Warning($"Character Source texture is null {bodyPartType}");
		}
		if (Channel != 0)
		{
			PFLog.Default.Warning($"Can't add NonDiffuse texture as primary {bodyPartType} {textureDesc.GetMainTextureName()}");
		}
		if (textureDesc.Channel != Channel)
		{
			PFLog.Default.Warning($"Wrong texture channel {bodyPartType} {textureDesc.GetMainTextureName()}!");
			return;
		}
		SortedSet<BodyPartType> value;
		bool flag = m_TexturesTypesMap.TryGetValue(textureDesc.GetSourceTexture(), out value);
		if (flag && value.Contains(bodyPartType))
		{
			m_TexturesTypesMap.Remove(textureDesc.GetSourceTexture());
			flag = false;
		}
		if (!flag)
		{
			value = new SortedSet<BodyPartType>(BodyPartTypeNameComparer.Instance);
			m_TexturesTypesMap[textureDesc.GetSourceTexture()] = value;
			m_SortedTextures.Add(textureDesc);
		}
		value.Add(bodyPartType);
	}

	public void AddSecondaryTexture(CharacterTextureDescription textureDesc, Texture primaryTexture, BodyPartType bodyPartType)
	{
		if (textureDesc.GetSourceTexture().width != primaryTexture.width || textureDesc.GetSourceTexture().height != primaryTexture.height)
		{
			PFLog.Default.Error("Textures must be the same size " + textureDesc.GetMainTextureName() + " - " + primaryTexture.name);
		}
		if (!m_TexturesTypesMap.TryGetValue(primaryTexture, out var value))
		{
			value = new SortedSet<BodyPartType>(BodyPartTypeNameComparer.Instance);
			m_TexturesTypesMap[primaryTexture] = value;
			m_SortedTextures.Add(textureDesc);
		}
		value.Add(bodyPartType);
		m_PrimaryTextureMap.TryAdd(textureDesc, primaryTexture);
	}

	public Texture Build(EquipmentEntity.PaintedTextures paintedTextures, MaterialProperties materialProperties)
	{
		CalculateRects();
		bool isSwitch = Application.platform == RuntimePlatform.Switch2;
		RenderTextureReadWrite colorSpace = ((Channel != 0) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
		RenderTexture renderTexture = CreateAtlasRenderTexture(colorSpace, isSwitch);
		renderTexture.name = $"{Channel}_RT";
		RenderTexture renderTexture2 = CreatePreviousRenderTexture(colorSpace, isSwitch);
		renderTexture2.name = $"{Channel}_prevRT";
		RenderTexture active = RenderTexture.active;
		Color backgroundColor;
		switch (Channel)
		{
		case CharacterTextureChannel.Diffuse:
			BakeMaterial.DisableKeyword("ALPHA_MASK_ON");
			BakeMaterial.DisableKeyword("NORMAL_MAP_ON");
			backgroundColor = Color.clear;
			break;
		case CharacterTextureChannel.Normal:
			BakeMaterial.EnableKeyword("ALPHA_MASK_ON");
			BakeMaterial.EnableKeyword("NORMAL_MAP_ON");
			backgroundColor = new Color(0.5f, 0.5f, 0f, 0f);
			break;
		case CharacterTextureChannel.Masks:
			BakeMaterial.EnableKeyword("ALPHA_MASK_ON");
			BakeMaterial.DisableKeyword("NORMAL_MAP_ON");
			BakeMaterial.SetFloat(ShaderProps._Roughness, materialProperties.Roughness);
			BakeMaterial.SetFloat(ShaderProps._Emission, materialProperties.Emission);
			BakeMaterial.SetFloat(ShaderProps._Metallic, materialProperties.Metallic);
			backgroundColor = new Color(0.85f, 0f, 0f, 0f);
			break;
		default:
			backgroundColor = Color.clear;
			break;
		}
		RenderTexture.active = renderTexture2;
		GL.Clear(clearDepth: false, clearColor: true, backgroundColor);
		RenderTexture.active = renderTexture;
		GL.Clear(clearDepth: false, clearColor: true, backgroundColor);
		BakeMaterial.SetTexture(ShaderProps._PreviousTex, renderTexture2);
		foreach (CharacterTextureDescription sortedTexture in m_SortedTextures)
		{
			Rect rect = ((Channel == CharacterTextureChannel.Diffuse) ? m_Rects[sortedTexture.GetSourceTexture()] : m_Rects[m_PrimaryTextureMap[sortedTexture]]);
			float num = (float)sortedTexture.DiffuseTexture.width / m_ScaleFactor.x;
			float num2 = (float)sortedTexture.DiffuseTexture.height / m_ScaleFactor.y;
			Vector4 value = new Vector4(0f, 0f, rect.width / num, rect.height / num2);
			Vector4 value2 = new Vector4(rect.x / m_Size.x, rect.y / m_Size.y, rect.width / m_Size.x, rect.height / m_Size.y);
			Vector2 vector = new Vector2(1f / m_Size.x, 1f / m_Size.y);
			BakeMaterial.SetVector(ShaderProps._SrcRect, value);
			BakeMaterial.SetVector(ShaderProps._DstRect, value2);
			BakeMaterial.SetTexture(ShaderProps._AlphaMask, sortedTexture.DiffuseTexture);
			BakeMaterial.SetVector(ShaderProps._TexelSize, vector);
			if (sortedTexture.FillTextureBackgroundWithColor)
			{
				BakeMaterial.EnableKeyword("COLORIZE_BACKGROUND");
			}
			else
			{
				BakeMaterial.DisableKeyword("COLORIZE_BACKGROUND");
			}
			Texture source = null;
			switch (Channel)
			{
			case CharacterTextureChannel.Diffuse:
				ShadowBakeMaterial.SetVector(ShaderProps._SrcRect, value);
				ShadowBakeMaterial.SetVector(ShaderProps._DstRect, value2);
				DiffuseBakeMaterial.SetVector(ShaderProps._SrcRect, value);
				DiffuseBakeMaterial.SetVector(ShaderProps._DstRect, value2);
				source = (Texture)(((object)paintedTextures.Get(sortedTexture)) ?? ((object)sortedTexture.DiffuseTexture));
				switch (sortedTexture.DiffuseBlendMode)
				{
				case CharacterTextureDescription.BlendMode.Add:
					SetBlendingMethod(BakeMaterial, m_DiffuseBlendingKeywords[0]);
					break;
				case CharacterTextureDescription.BlendMode.Multiply:
					SetBlendingMethod(BakeMaterial, m_DiffuseBlendingKeywords[1]);
					break;
				case CharacterTextureDescription.BlendMode.Overlay:
					SetBlendingMethod(BakeMaterial, m_DiffuseBlendingKeywords[2]);
					break;
				case CharacterTextureDescription.BlendMode.Screen:
					SetBlendingMethod(BakeMaterial, m_DiffuseBlendingKeywords[3]);
					break;
				}
				if (sortedTexture.ColorizerMasks != null)
				{
					ShadowBakeMaterial.SetTexture(ShaderProps._Mask, sortedTexture.ColorizerMasks);
					Graphics.Blit(source, renderTexture, ShadowBakeMaterial);
					if (sortedTexture.DiffuseTexture == null)
					{
						continue;
					}
					Graphics.Blit(source, renderTexture, DiffuseBakeMaterial);
					Graphics.Blit(renderTexture, renderTexture2);
				}
				break;
			case CharacterTextureChannel.Normal:
			{
				source = sortedTexture.NormalTexture;
				Texture2D value3 = ((sortedTexture.AnisotropyMasks != null) ? sortedTexture.AnisotropyMasks : ConfigRoot.Instance.Prefabs.DefaultCharAnisotropyFallbackTexture);
				BakeMaterial.SetTexture(ShaderProps._AnisotropyTex, value3);
				break;
			}
			case CharacterTextureChannel.Masks:
				RoughnessLightenBlend.SetVector(ShaderProps._SrcRect, value);
				RoughnessLightenBlend.SetVector(ShaderProps._DstRect, value2);
				source = sortedTexture.MaskTexture;
				if (sortedTexture.ColorizerMasks != null)
				{
					Graphics.Blit(sortedTexture.ColorizerMasks, renderTexture, RoughnessLightenBlend);
					Graphics.Blit(renderTexture, renderTexture2);
				}
				break;
			}
			Graphics.Blit(source, renderTexture, BakeMaterial);
			Graphics.Blit(renderTexture, renderTexture2);
		}
		RenderTexture.active = active;
		if (m_UncompressedTexture != null && !m_CompressingNow)
		{
			m_UncompressedTexture.Release();
			UnityEngine.Object.Destroy(m_UncompressedTexture);
		}
		m_UncompressedTexture = renderTexture;
		return renderTexture;
	}

	private void SetBlendingMethod(Material mat, string enable)
	{
		string[] diffuseBlendingKeywords = m_DiffuseBlendingKeywords;
		foreach (string text in diffuseBlendingKeywords)
		{
			if (text == enable)
			{
				mat.EnableKeyword(text);
			}
			else
			{
				mat.DisableKeyword(text);
			}
		}
	}

	private RenderTexture CreatePreviousRenderTexture(RenderTextureReadWrite colorSpace, bool isSwitch2)
	{
		if (colorSpace == RenderTextureReadWrite.sRGB)
		{
			if (m_TempSRGBTexture != null && (m_TempSRGBTexture.width != (int)m_Size.x || m_TempSRGBTexture.height != (int)m_Size.y))
			{
				m_TempSRGBTexture.Release();
				m_TempSRGBTexture = null;
			}
			if (m_TempSRGBTexture == null)
			{
				m_TempSRGBTexture = CreateAtlasRenderTexture(colorSpace, isSwitch2);
			}
			m_TempSRGBTexture.DiscardContents();
			return m_TempSRGBTexture;
		}
		if (m_TempLinearTexture != null && (m_TempLinearTexture.width != (int)m_Size.x || m_TempLinearTexture.height != (int)m_Size.y))
		{
			m_TempLinearTexture.Release();
			m_TempLinearTexture = null;
		}
		if (m_TempLinearTexture == null)
		{
			m_TempLinearTexture = CreateAtlasRenderTexture(colorSpace, isSwitch2);
		}
		m_TempLinearTexture.DiscardContents();
		return m_TempLinearTexture;
	}

	private RenderTexture CreateAtlasRenderTexture(RenderTextureReadWrite colorSpace, bool isSwitch2)
	{
		return new RenderTexture((int)m_Size.x, (int)m_Size.y, 0, RenderTextureFormat.ARGB32, colorSpace)
		{
			filterMode = FilterMode.Bilinear,
			wrapMode = TextureWrapMode.Repeat,
			anisoLevel = 1,
			useMipMap = !isSwitch2,
			autoGenerateMips = !isSwitch2
		};
	}

	public void CompressAsync(Action<CharacterAtlas, Texture2D> onTextureCompressed, Action<CharacterAtlas> onTextureNotCompressed)
	{
		DxtCompressorService2 dxtCompressorService = Services.GetInstance<DxtCompressorService2>();
		if (m_UncompressedTexture == null)
		{
			onTextureNotCompressed(this);
		}
		else if (dxtCompressorService != null)
		{
			CompressAndHandle();
		}
		else
		{
			onTextureNotCompressed?.Invoke(this);
		}
		async void CompressAndHandle()
		{
			RenderTexture textureIn = m_UncompressedTexture;
			m_CompressingNow = true;
			try
			{
				Texture2D texture2D = await dxtCompressorService.CompressTexture(textureIn);
				if (m_UncompressedTexture != textureIn)
				{
					UnityEngine.Object.Destroy(texture2D);
					onTextureNotCompressed?.Invoke(this);
				}
				else
				{
					AtlasTexture = texture2D;
					onTextureCompressed?.Invoke(this, texture2D);
				}
			}
			catch (Exception ex)
			{
				PFLog.Default.Error(ex, "Failed to compress atlas to DXT");
				onTextureNotCompressed?.Invoke(this);
			}
			finally
			{
				m_CompressingNow = false;
				textureIn.Release();
				UnityEngine.Object.Destroy(textureIn);
			}
		}
	}

	public void ClearTempValues()
	{
		if (m_UncompressedTexture != null && !m_CompressingNow)
		{
			m_UncompressedTexture.Release();
			UnityEngine.Object.Destroy(m_UncompressedTexture);
		}
		m_UncompressedTexture = null;
	}

	private void CalculateRects()
	{
		m_Rects.Clear();
		Dictionary<BodyPartType, Rect> dictionary = new Dictionary<BodyPartType, Rect>();
		HashSet<Texture> hashSet = new HashSet<Texture>();
		foreach (CharacterTextureDescription sortedTexture in m_SortedTextures)
		{
			Texture value;
			Texture texture = (m_PrimaryTextureMap.TryGetValue(sortedTexture, out value) ? value : sortedTexture.GetSourceTexture());
			if (!hashSet.Add(texture) || !m_TexturesTypesMap.TryGetValue(texture, out var value2))
			{
				continue;
			}
			bool flag = false;
			Rect value3 = new Rect(0f, 0f, 0f, 0f);
			foreach (BodyPartType item in value2)
			{
				flag = dictionary.TryGetValue(item, out value3);
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				foreach (BodyPartType item2 in value2)
				{
					value3 = (dictionary[item2] = GetMappedRect(item2));
				}
			}
			m_Rects[texture] = value3;
		}
	}

	private Rect GetMappedRect(BodyPartType type)
	{
		Rect result = Rect.zero;
		foreach (CharacterAtlasData.BodyPartCoords bodyPartsCoord in m_CharacterAtlasDataLocal.BodyPartsCoords)
		{
			if (bodyPartsCoord.bodyPart.Name == type.Name)
			{
				result = new Rect(bodyPartsCoord.gpuCoords.x, bodyPartsCoord.gpuCoords.y, bodyPartsCoord.textureRectCoords.width, bodyPartsCoord.textureRectCoords.height);
			}
		}
		result.x /= m_ScaleFactor.x;
		result.y /= m_ScaleFactor.y;
		result.width /= m_ScaleFactor.x;
		result.height /= m_ScaleFactor.y;
		return result;
	}

	public void Cleanup()
	{
		if (AtlasTexture != null)
		{
			UnityEngine.Object.Destroy(AtlasTexture);
		}
	}
}
