using System;
using Kingmaker.Code.UI.Common.SmartSliders;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class HealthBarWidget : MonoBehaviour
{
	[Serializable]
	private struct WidgetPart
	{
		public GameObject Root;

		public Slider ValueSlider;

		public FilledRangedSlider DamageSlider;

		public TMP_Text Text;
	}

	[SerializeField]
	private WidgetPart m_Health;

	[SerializeField]
	private WidgetPart m_Armor;

	[Space]
	[SerializeField]
	private GameObject m_ArmorBackground;

	[SerializeField]
	private GameObject m_HealthBackground;

	[SerializeField]
	private Graphic[] m_ColoredGraphics;

	[Space]
	[SerializeField]
	private Color m_PlayerColor;

	[SerializeField]
	private Color m_AllyColor;

	[SerializeField]
	private Color m_EnemyColor;

	private bool m_HealthSliderInitialized;

	private bool m_ArmorSliderInitialized;

	public void SetHealthValue(int current, int max, int damage)
	{
		if (!m_HealthSliderInitialized)
		{
			m_Health.DamageSlider.Initialize();
		}
		SetValue(m_Health, current, max, damage);
	}

	public void SetArmorValue(int current, int max, int damage)
	{
		if (!m_ArmorSliderInitialized)
		{
			m_Armor.DamageSlider.Initialize();
		}
		SetValue(m_Armor, current, max, damage);
	}

	public void SetHasArmor(bool hasArmor)
	{
		m_Armor.Root.SetActive(hasArmor);
		m_ArmorBackground.SetActive(hasArmor);
		m_HealthBackground.SetActive(!hasArmor);
	}

	public void SetAsPlayer()
	{
		SetColor(m_PlayerColor);
	}

	public void SetAsAlly()
	{
		SetColor(m_AllyColor);
	}

	public void SetAsEnemy()
	{
		SetColor(m_EnemyColor);
	}

	private void SetColor(Color color)
	{
		Graphic[] coloredGraphics = m_ColoredGraphics;
		for (int i = 0; i < coloredGraphics.Length; i++)
		{
			coloredGraphics[i].color = color;
		}
	}

	private void SetValue(WidgetPart part, int current, int max, int damage)
	{
		part.Text.SetText(current.ToString());
		float num = (float)current / (float)max;
		float from = (float)Mathf.Max(0, current - damage) / (float)max;
		part.ValueSlider.normalizedValue = num;
		part.DamageSlider.SetRange(from, num, blink: true);
	}
}
