using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.Components.Camera;

public class UIPostProcessSpace : UIBehaviour
{
	private class TargetImage : IDisposable
	{
		public readonly RawImage Target;

		public TargetImage(RawImage target)
		{
			Target = target;
		}

		public void Dispose()
		{
			Target.texture = null;
		}
	}

	private static UIPostProcessSpace s_Instance;

	private static readonly int Min = Shader.PropertyToID("_Min");

	private static readonly int AlphaUV = Shader.PropertyToID("_AlphaUV");

	private readonly List<TargetImage> m_Targets = new List<TargetImage>();

	private RenderTexture m_SharedRenderTexture;

	public static UIPostProcessSpace Instance => s_Instance;

	public void Bind()
	{
		s_Instance = this;
		UICamera.AdditionalUICamera.enabled = false;
	}

	public void Push(RawImage targetImage)
	{
		TargetImage targetImage2 = new TargetImage(targetImage);
		m_Targets.Add(targetImage2);
		SetupSharedRenderTarget(targetImage2);
		UICamera.AdditionalUICamera.enabled = true;
	}

	public void Pop(RawImage targetImage)
	{
		TargetImage targetImage2 = m_Targets.FirstOrDefault((TargetImage t) => t.Target == targetImage);
		if (targetImage2 != null)
		{
			m_Targets.Remove(targetImage2);
			if (m_Targets.Count == 0)
			{
				UICamera.AdditionalUICamera.enabled = false;
			}
		}
	}

	public void Dispose()
	{
		m_Targets.ForEach(delegate(TargetImage pair)
		{
			pair.Dispose();
		});
		m_Targets.Clear();
		if ((bool)m_SharedRenderTexture)
		{
			m_SharedRenderTexture.Release();
			m_SharedRenderTexture = null;
		}
		s_Instance = null;
		if ((bool)UICamera.AdditionalUICamera)
		{
			UICamera.AdditionalUICamera.enabled = false;
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		foreach (TargetImage target in m_Targets)
		{
			SetupSharedRenderTarget(target);
		}
	}

	private void SetupSharedRenderTarget(TargetImage pair)
	{
		int pixelWidth = UICamera.MainUICamera.pixelWidth;
		int pixelHeight = UICamera.MainUICamera.pixelHeight;
		if (!m_SharedRenderTexture || m_SharedRenderTexture.width != pixelWidth || m_SharedRenderTexture.height != pixelHeight)
		{
			if ((bool)m_SharedRenderTexture)
			{
				m_SharedRenderTexture.Release();
				m_SharedRenderTexture = null;
			}
			m_SharedRenderTexture = new RenderTexture(pixelWidth, pixelHeight, 24, RenderTextureFormat.ARGBHalf)
			{
				name = $"UIPostProcess_{pixelWidth}x{pixelHeight}_ARGBHalf"
			};
		}
		pair.Target.texture = m_SharedRenderTexture;
		UICamera.AdditionalUICamera.targetTexture = m_SharedRenderTexture;
		UpdateUVRect(pair);
		UpdateAlphaMask(pair);
	}

	private void UpdateUVRect(TargetImage pair)
	{
		RawImage target = pair.Target;
		if (!target.texture)
		{
			Debug.LogWarning("RawImage has no texture assigned.");
			return;
		}
		int width = target.texture.width;
		int height = target.texture.height;
		float num = (float)width / (float)height;
		Rect rect = target.rectTransform.rect;
		float num2 = rect.width / rect.height;
		Rect uvRect = new Rect(0f, 0f, 1f, 1f);
		if (num > num2)
		{
			float num3 = num2 / num;
			uvRect.x = (1f - num3) / 2f;
			uvRect.width = num3;
		}
		else if (num < num2)
		{
			float num4 = num / num2;
			uvRect.y = (1f - num4) / 2f;
			uvRect.height = num4;
		}
		target.uvRect = uvRect;
	}

	private void UpdateAlphaMask(TargetImage pair)
	{
		RawImage target = pair.Target;
		if ((bool)target && (bool)target.texture)
		{
			Material material = target.material;
			if ((bool)material && material.HasProperty(Min) && material.HasProperty(AlphaUV))
			{
				Rect uvRect = target.uvRect;
				material.SetVector(Min, new Vector2(uvRect.x, uvRect.y));
				material.SetVector(AlphaUV, new Vector2(uvRect.width, uvRect.height));
			}
		}
	}
}
