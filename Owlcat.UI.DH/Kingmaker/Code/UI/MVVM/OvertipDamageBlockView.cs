using Kingmaker.UnitLogic.Abilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipDamageBlockView : View<OvertipDamageBlockVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_Damage;

	[SerializeField]
	private Color m_HealPredictionColor;

	[SerializeField]
	private Color m_DamagePredictionColor;

	public void HideInstant()
	{
		m_CanvasGroup.alpha = 0f;
	}

	protected override void OnBind()
	{
		base.ViewModel.IsVisibleTrigger.CombineLatest(base.ViewModel.HasHit, base.ViewModel.MechanicEntityUIState.HoverSelfTargetAbility, base.ViewModel.MechanicEntityUIState.Ability, (bool isVisible, bool hasHit, bool hoverSelf, AbilityData ability) => new { isVisible, hasHit, hoverSelf, ability }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(value =>
		{
			bool flag = ((value.isVisible && value.hasHit) || value.hoverSelf) && (base.ViewModel.MinDamage.CurrentValue > 0 || base.ViewModel.MaxDamage.CurrentValue > 0);
			m_CanvasGroup.alpha = (flag ? 1f : 0f);
		})
			.AddTo(this);
		base.ViewModel.MinDamage.CombineLatest(base.ViewModel.MaxDamage, (int min, int max) => new { min, max }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(value =>
		{
			Color color = (base.ViewModel.IsHeal ? m_HealPredictionColor : m_DamagePredictionColor);
			m_Damage.color = color;
			m_Damage.text = $"{value.min}–{value.max}";
		})
			.AddTo(this);
	}
}
