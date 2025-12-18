using System;
using Code.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class DOTEffectIconWidget : MonoBehaviour
{
	[Serializable]
	private struct DOTVisualSettings
	{
		public Sprite Sprite;

		public Color Color;
	}

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private DOTVisualSettings m_ToxicSettings;

	[SerializeField]
	private DOTVisualSettings m_BurningSettings;

	[SerializeField]
	private DOTVisualSettings m_BleedingSettings;

	public void SetDOTType(DOT dotType)
	{
		DOTVisualSettings settings;
		bool flag = TryGetSettings(dotType, out settings);
		if (flag)
		{
			m_Icon.sprite = settings.Sprite;
			m_Icon.color = settings.Color;
		}
		SetActive(flag);
	}

	public void SetActive(bool isActive)
	{
		base.gameObject.SetActive(isActive);
		if (!isActive)
		{
			m_Icon.sprite = null;
		}
	}

	private bool TryGetSettings(DOT dot, out DOTVisualSettings settings)
	{
		settings = dot switch
		{
			DOT.Bleeding => m_BleedingSettings, 
			DOT.Burning => m_BurningSettings, 
			DOT.Toxic => m_ToxicSettings, 
			_ => default(DOTVisualSettings), 
		};
		return !settings.Equals(null);
	}
}
