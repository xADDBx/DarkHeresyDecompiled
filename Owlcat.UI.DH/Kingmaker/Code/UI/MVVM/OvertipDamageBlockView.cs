using System;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipDamageBlockView : View<OvertipDamageBlockVM>
{
	[Serializable]
	private struct PredictionIcon
	{
		public Sprite Sprite;

		public Color Color;
	}

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_Damage;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Color m_HealPredictionColor;

	[SerializeField]
	private Color m_DamagePredictionColor;

	[SerializeField]
	private PredictionIcon m_VitalDamageIcon;

	[SerializeField]
	private PredictionIcon m_HealthDamageIcon;

	[SerializeField]
	private PredictionIcon m_ArmorDamageIcon;

	[SerializeField]
	private PredictionIcon m_HealIcon;

	[SerializeField]
	private PredictionIcon m_ArmorRepairIcon;

	public void HideInstant()
	{
		m_CanvasGroup.alpha = 0f;
	}

	protected override void OnBind()
	{
		base.ViewModel.IsVisible.CombineLatest(base.ViewModel.HasHit, base.ViewModel.EntityUIState.HoverSelfTargetAbility, base.ViewModel.EntityUIState.Ability, (bool isVisible, bool hasHit, bool hoverSelf, AbilityData ability) => new { isVisible, hasHit, hoverSelf, ability }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(value =>
		{
			OvertipDamageBlockVM.PredictionData currentValue = base.ViewModel.Prediction.CurrentValue;
			bool flag = ((value.isVisible && value.hasHit) || value.hoverSelf) && (currentValue.Min > 0 || currentValue.Max > 0);
			m_CanvasGroup.alpha = (flag ? 1f : 0f);
		})
			.AddTo(this);
		base.ViewModel.Prediction.Subscribe(HandlePredictionChanged).AddTo(this);
	}

	private void HandlePredictionChanged(OvertipDamageBlockVM.PredictionData prediction)
	{
		Color color = (prediction.IsHeal ? m_HealPredictionColor : m_DamagePredictionColor);
		m_Damage.color = color;
		m_Damage.text = $"{prediction.Min}–{prediction.Max}";
		if (!TryGetSprite(prediction, out var icon))
		{
			m_Icon.gameObject.SetActive(value: false);
			return;
		}
		m_Icon.sprite = icon.Sprite;
		m_Icon.color = icon.Color;
		m_Icon.gameObject.SetActive(value: true);
	}

	private bool TryGetSprite(OvertipDamageBlockVM.PredictionData prediction, out PredictionIcon icon)
	{
		PredictionIcon predictionIcon;
		switch (prediction.IconType)
		{
		case OvertipDamageBlockVM.IconType.Vital:
			if (!prediction.IsHeal)
			{
				predictionIcon = m_VitalDamageIcon;
				break;
			}
			goto default;
		case OvertipDamageBlockVM.IconType.Health:
			predictionIcon = (prediction.IsHeal ? m_HealIcon : m_HealthDamageIcon);
			break;
		case OvertipDamageBlockVM.IconType.Armor:
			predictionIcon = (prediction.IsHeal ? m_ArmorRepairIcon : m_ArmorDamageIcon);
			break;
		default:
			predictionIcon = default(PredictionIcon);
			break;
		}
		icon = predictionIcon;
		return icon.Sprite != null;
	}
}
