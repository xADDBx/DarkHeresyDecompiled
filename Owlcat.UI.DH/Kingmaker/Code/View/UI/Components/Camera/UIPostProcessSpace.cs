using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.Components.Camera;

public class UIPostProcessSpace : UIBehaviour
{
	private class TargetsPair
	{
		public Transform Target;

		public Transform TargetParent;

		public RawImage TargetImage;
	}

	private static UIPostProcessSpace s_Instance;

	private static readonly int Min = Shader.PropertyToID("_Min");

	private static readonly int AlphaUV = Shader.PropertyToID("_AlphaUV");

	[Header("Elements")]
	[SerializeField]
	private Transform m_Container;

	[SerializeField]
	private Canvas m_Canvas;

	private readonly List<TargetsPair> m_Targets = new List<TargetsPair>();

	private RenderTexture m_SharedRenderTexture;

	private List<TextMeshProUGUI> m_TMPs;

	public static UIPostProcessSpace Instance => s_Instance;

	public void Bind()
	{
		s_Instance = this;
		UICamera.AdditionalUICamera.enabled = false;
	}

	public void Push(Transform target, Transform targetParent, RawImage targetImage)
	{
		TargetsPair targetsPair = new TargetsPair
		{
			Target = target,
			TargetParent = targetParent,
			TargetImage = targetImage
		};
		m_Targets.Add(targetsPair);
		int sortingOrder = target.GetComponentInParent<Canvas>().sortingOrder;
		m_Canvas.sortingOrder = sortingOrder + 1;
		target.SetParent(m_Container, worldPositionStays: false);
		SetupSharedRenderTarget(targetsPair);
		UICamera.AdditionalUICamera.enabled = true;
	}

	public void Pop(Transform target)
	{
		TargetsPair targetsPair = m_Targets.FirstOrDefault((TargetsPair t) => t.Target == target);
		if (targetsPair != null)
		{
			targetsPair.Target.SetParent(targetsPair.TargetParent, worldPositionStays: false);
			targetsPair.TargetImage.texture = null;
			m_Targets.Remove(targetsPair);
			UICamera.AdditionalUICamera.enabled = false;
			m_TMPs?.Clear();
			m_TMPs = null;
		}
	}

	public void Dispose()
	{
		foreach (TargetsPair target in m_Targets)
		{
			if ((bool)target.Target && (bool)target.TargetParent)
			{
				target.Target.SetParent(target.TargetParent, worldPositionStays: false);
			}
			if ((bool)target.TargetImage)
			{
				target.TargetImage.texture = null;
			}
		}
		m_Targets.Clear();
		if ((bool)m_SharedRenderTexture)
		{
			m_SharedRenderTexture.Release();
			m_SharedRenderTexture = null;
		}
		s_Instance = null;
		m_TMPs?.Clear();
		m_TMPs = null;
		if ((bool)UICamera.AdditionalUICamera)
		{
			UICamera.AdditionalUICamera.enabled = false;
		}
	}

	public void ForceTextMeshProRedrawLayout(bool force)
	{
		if (m_TMPs == null || force)
		{
			m_TMPs = m_Canvas.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true).ToList();
		}
		foreach (TextMeshProUGUI tMP in m_TMPs)
		{
			tMP.SetVerticesDirty();
			tMP.SetLayoutDirty();
		}
	}

	public void ForceTextMeshProRedrawMesh(bool force)
	{
		if (m_TMPs == null || force)
		{
			m_TMPs = m_Canvas.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true).ToList();
		}
		foreach (TextMeshProUGUI tMP in m_TMPs)
		{
			tMP.ForceMeshUpdate();
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		foreach (TargetsPair target in m_Targets)
		{
			SetupSharedRenderTarget(target);
		}
	}

	private void SetupSharedRenderTarget(TargetsPair pair)
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
		pair.TargetImage.texture = m_SharedRenderTexture;
		UICamera.AdditionalUICamera.targetTexture = m_SharedRenderTexture;
		UpdateUVRect(pair);
		UpdateAlphaMask(pair);
	}

	private void UpdateUVRect(TargetsPair pair)
	{
		RawImage targetImage = pair.TargetImage;
		if (!targetImage.texture)
		{
			Debug.LogWarning("RawImage has no texture assigned.");
			return;
		}
		int width = targetImage.texture.width;
		int height = targetImage.texture.height;
		float num = (float)width / (float)height;
		Rect rect = targetImage.rectTransform.rect;
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
		targetImage.uvRect = uvRect;
	}

	private void UpdateAlphaMask(TargetsPair pair)
	{
		RawImage targetImage = pair.TargetImage;
		if ((bool)targetImage && (bool)targetImage.texture)
		{
			Material material = targetImage.material;
			if ((bool)material && material.HasProperty(Min) && material.HasProperty(AlphaUV))
			{
				Rect uvRect = targetImage.uvRect;
				material.SetVector(Min, new Vector2(uvRect.x, uvRect.y));
				material.SetVector(AlphaUV, new Vector2(uvRect.width, uvRect.height));
			}
		}
	}
}
