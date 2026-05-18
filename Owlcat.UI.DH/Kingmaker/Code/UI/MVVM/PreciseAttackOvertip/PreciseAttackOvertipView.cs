using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.PreciseAttackOvertip;

public class PreciseAttackOvertipView : View<PreciseAttackOvertipVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private HealthBarWidget m_HealthBar;

	protected override void OnBind()
	{
		base.ViewModel.OvertipData.Subscribe(HandleOvertipDataChanged).AddTo(this);
	}

	private void HandleOvertipDataChanged(OvertipData data)
	{
		if (data.Equals(default(OvertipData)))
		{
			m_CanvasGroup.alpha = 0f;
			return;
		}
		m_HealthBar.SetHealthValue(data.HealthLeft, data.HealthMax, data.HealthDamage);
		m_HealthBar.SetElementsVisibility(data.HasArmor, !base.ViewModel.IsCountHpAsArmor);
		if (data.HasArmor)
		{
			m_HealthBar.SetArmorValue(data.ArmorLeft, data.ArmorMax, data.ArmorDamage);
		}
		switch (data.Faction)
		{
		case Faction.Player:
			m_HealthBar.SetAsPlayer();
			break;
		case Faction.Enemy:
			m_HealthBar.SetAsEnemy();
			break;
		default:
			m_HealthBar.SetAsAlly();
			break;
		}
		m_CanvasGroup.alpha = 1f;
	}
}
