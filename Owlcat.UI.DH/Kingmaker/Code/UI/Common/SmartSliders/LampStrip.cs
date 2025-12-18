using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.Common.SmartSliders;

public class LampStrip : MonoBehaviour
{
	[Header("Animation")]
	[SerializeField]
	private float m_BlinkTime;

	[FormerlySerializedAs("m_ContainerWidth")]
	[Header("Layout")]
	[SerializeField]
	private float m_MaxContainerWidth = 240f;

	[SerializeField]
	private RectTransform m_LampsContainer;

	[SerializeField]
	private Lamp m_LampPrefab;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private LayoutElement m_OwnLayoutElement;

	[Header("Colors")]
	[SerializeField]
	private Color m_ForegroundColor = Color.white;

	[SerializeField]
	private Color m_BackgroundColor = Color.white;

	[Header("Spacing")]
	[Min(0f)]
	[SerializeField]
	private float m_LampSpacing = 2f;

	[Min(1f)]
	[SerializeField]
	private int m_GroupSize = 5;

	[Min(0f)]
	[SerializeField]
	private float m_GroupSpacing = 10f;

	[SerializeField]
	private float m_MaxWidth = 20f;

	private readonly List<Lamp> m_Lamps = new List<Lamp>();

	private Tweener m_BlinkTweener;

	private float m_CurrentValue;

	private int m_LampCount;

	public int LampCount => m_LampCount;

	public void SetLampCount(int count)
	{
		if ((bool)m_LampsContainer && (bool)m_LampPrefab)
		{
			m_LampCount = Mathf.Max(0, count);
			EnsurePoolSize(m_LampCount);
			UpdateActiveLamps(m_LampCount);
			LayoutLamps(m_LampCount);
		}
	}

	public void SetVisibleRange(int fromIndex, int toIndex)
	{
		fromIndex = Mathf.Max(0, fromIndex);
		toIndex = Mathf.Min(m_LampCount, toIndex);
		if (toIndex < fromIndex)
		{
			int num = toIndex;
			int num2 = fromIndex;
			fromIndex = num;
			toIndex = num2;
		}
		for (int i = 0; i < m_LampCount; i++)
		{
			Lamp lamp = m_Lamps[i];
			bool flag = i >= fromIndex && i < toIndex;
			if (flag)
			{
				lamp.SetColors(m_ForegroundColor, m_BackgroundColor);
			}
			lamp.SetVisible(flag);
		}
	}

	public void Blink()
	{
		m_BlinkTweener?.Kill();
		if ((bool)m_CanvasGroup)
		{
			m_CanvasGroup.alpha = 1f;
			m_BlinkTweener = m_CanvasGroup.DOFade(0f, m_BlinkTime).SetLoops(-1, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true)
				.SetAutoKill(autoKillOnCompletion: true);
		}
	}

	public void StopBlink()
	{
		m_BlinkTweener?.Kill();
		m_BlinkTweener = null;
		if ((bool)m_CanvasGroup)
		{
			m_CanvasGroup.alpha = 0f;
		}
	}

	public void HideAll()
	{
		for (int i = 0; i < m_Lamps.Count; i++)
		{
			m_Lamps[i].SetVisible(visible: false);
		}
	}

	private void EnsurePoolSize(int requiredCount)
	{
		if (m_Lamps.Count == 0)
		{
			CollectExistingLamps();
		}
		while (m_Lamps.Count < requiredCount)
		{
			CreateLampInstance();
		}
	}

	private void CollectExistingLamps()
	{
		foreach (Transform item in m_LampsContainer)
		{
			Lamp component = item.GetComponent<Lamp>();
			if (component != null)
			{
				RegisterLamp(component);
			}
		}
	}

	private void CreateLampInstance()
	{
		Lamp lamp = Object.Instantiate(m_LampPrefab, m_LampsContainer, worldPositionStays: false);
		lamp.gameObject.SetActive(value: false);
		RegisterLamp(lamp);
	}

	private void RegisterLamp(Lamp lamp)
	{
		ConfigureLampRect(lamp.RectTransform);
		lamp.SetColors(m_ForegroundColor, m_BackgroundColor);
		m_Lamps.Add(lamp);
	}

	private void ConfigureLampRect(RectTransform rt)
	{
		rt.anchorMin = new Vector2(0f, 0.5f);
		rt.anchorMax = new Vector2(0f, 0.5f);
		rt.pivot = new Vector2(0f, 0.5f);
	}

	private void UpdateActiveLamps(int count)
	{
		for (int i = 0; i < m_Lamps.Count; i++)
		{
			Lamp lamp = m_Lamps[i];
			bool flag = i < count;
			lamp.gameObject.SetActive(flag);
			if (!flag)
			{
				lamp.SetVisible(visible: false);
			}
		}
	}

	private void LayoutLamps(int count)
	{
		if (count <= 0)
		{
			return;
		}
		float maxContainerWidth = m_MaxContainerWidth;
		if (!(maxContainerWidth <= 0f))
		{
			int num = Mathf.CeilToInt((float)count / (float)m_GroupSize);
			int num2 = Mathf.Max(0, num - 1);
			float num3 = (float)(count - num) * m_LampSpacing + (float)num2 * m_GroupSpacing;
			float num4 = Mathf.Min(Mathf.Floor((maxContainerWidth - num3) / (float)count), m_MaxWidth);
			if (num4 < 0f)
			{
				num4 = 0f;
			}
			float num5 = 0f;
			for (int i = 0; i < count; i++)
			{
				RectTransform rectTransform = m_Lamps[i].RectTransform;
				rectTransform.sizeDelta = new Vector2(num4, rectTransform.sizeDelta.y);
				rectTransform.anchoredPosition = new Vector2(Mathf.Round(num5), 0f);
				num5 += num4;
				bool flag = (i + 1) % m_GroupSize == 0 && i + 1 < count;
				num5 += (flag ? m_GroupSpacing : m_LampSpacing);
			}
			LayoutElement ownLayoutElement = m_OwnLayoutElement;
			float minWidth = (m_OwnLayoutElement.preferredWidth = num5);
			ownLayoutElement.minWidth = minWidth;
		}
	}
}
