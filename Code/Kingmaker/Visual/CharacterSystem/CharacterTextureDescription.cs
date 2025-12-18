using System;
using Kingmaker.Blueprints.Root;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[Serializable]
public class CharacterTextureDescription
{
	[Flags]
	public enum MaskingFlags
	{
		None = 0,
		WhiteMaskPrimary = 1,
		WhiteMaskSecondary = 2,
		ColorizerMaskPrimary = 4,
		ColorizerMaskSecondary = 8
	}

	public enum BlendMode
	{
		Add,
		Multiply,
		Overlay,
		Screen
	}

	public Texture2D DiffuseTexture;

	public Texture2D NormalTexture;

	public Texture2D MaskTexture;

	public Texture2D ColorizerMasks;

	public Texture2D AnisotropyMasks;

	public bool FillTextureBackgroundWithColor;

	public BlendMode DiffuseBlendMode;

	public MaskingFlags TexturesMaskingFlags;

	[HideInInspector]
	public CharacterTextureChannel Channel;

	private static Material s_RepaintMaterial;

	public Texture GetSourceTexture()
	{
		return DiffuseTexture;
	}

	public void Repaint(ref RenderTexture rtToPaint, (Texture2D primaryRamp, CharacterColorsProfile primaryProfile) primaryDescr, (Texture2D secondaryRamp, CharacterColorsProfile secondaryProfile) secondaryDescr)
	{
		if (DiffuseTexture == null)
		{
			if (Application.isEditor)
			{
				PFLog.TechArt.Warning("Missing texture in one of the EE");
			}
		}
		else
		{
			if (TexturesMaskingFlags == MaskingFlags.None || (primaryDescr.primaryRamp == null && secondaryDescr.secondaryRamp == null))
			{
				return;
			}
			Texture2D diffuseTexture = DiffuseTexture;
			if (rtToPaint == null)
			{
				rtToPaint = RenderTexture.GetTemporary(diffuseTexture.width, diffuseTexture.height, 0, RenderTextureFormat.ARGB32);
				rtToPaint.name = diffuseTexture.name + "_RT";
			}
			if (s_RepaintMaterial == null)
			{
				s_RepaintMaterial = new Material(Shader.Find("Hidden/CharacterTextureRepaint"));
			}
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = rtToPaint;
			try
			{
				Graphics.Blit(diffuseTexture, rtToPaint);
				if (primaryDescr.primaryRamp != null)
				{
					if (CheckMask(MaskingFlags.WhiteMaskPrimary))
					{
						s_RepaintMaterial.SetTexture(ShaderProps._Mask, ConfigRoot.Instance.Prefabs.DefaultCharWhiteMaskTexture);
					}
					if (ColorizerMasks != null && CheckMask(MaskingFlags.ColorizerMaskPrimary))
					{
						s_RepaintMaterial.SetTexture(ShaderProps._Mask, ColorizerMasks);
					}
					s_RepaintMaterial.SetFloat(ShaderProps._Specialmask, 1f);
					s_RepaintMaterial.SetFloat(ShaderProps._Mode, (float)primaryDescr.primaryProfile.Mode);
					s_RepaintMaterial.SetTexture(ShaderProps._Ramp, primaryDescr.primaryRamp);
					Graphics.Blit(diffuseTexture, rtToPaint, s_RepaintMaterial);
				}
				if (secondaryDescr.secondaryRamp != null)
				{
					RenderTexture temporary = RenderTexture.GetTemporary(diffuseTexture.width, diffuseTexture.height, 0, RenderTextureFormat.ARGB32);
					Graphics.Blit(rtToPaint, temporary);
					if (CheckMask(MaskingFlags.WhiteMaskSecondary))
					{
						s_RepaintMaterial.SetTexture(ShaderProps._Mask, ConfigRoot.Instance.Prefabs.DefaultCharWhiteMaskTexture);
					}
					if (ColorizerMasks != null && CheckMask(MaskingFlags.ColorizerMaskSecondary))
					{
						s_RepaintMaterial.SetTexture(ShaderProps._Mask, ColorizerMasks);
					}
					s_RepaintMaterial.SetFloat(ShaderProps._Specialmask, -1f);
					s_RepaintMaterial.SetFloat(ShaderProps._Mode, (float)secondaryDescr.secondaryProfile.Mode);
					s_RepaintMaterial.SetTexture(ShaderProps._Ramp, secondaryDescr.secondaryRamp);
					Graphics.Blit(diffuseTexture, temporary, s_RepaintMaterial);
					Graphics.Blit(temporary, rtToPaint);
					RenderTexture.ReleaseTemporary(temporary);
				}
			}
			finally
			{
				RenderTexture.active = active;
			}
		}
	}

	private bool CheckMask(MaskingFlags flag)
	{
		return (TexturesMaskingFlags & flag) == flag;
	}

	public string GetMainTextureName()
	{
		if (!DiffuseTexture)
		{
			return string.Empty;
		}
		return DiffuseTexture.name;
	}

	public void DestroySourceAssets()
	{
		if (DiffuseTexture != null)
		{
			UnityEngine.Object.DestroyImmediate(DiffuseTexture, allowDestroyingAssets: true);
		}
	}
}
