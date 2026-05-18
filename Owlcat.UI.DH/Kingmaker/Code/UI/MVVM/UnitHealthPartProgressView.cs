using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class UnitHealthPartProgressView : View<UnitHealthPartVM>
{
	[SerializeField]
	private CanvasGroup m_MainGroup;

	[SerializeField]
	private Image m_Health;

	[SerializeField]
	private Image m_Armor;

	[SerializeField]
	private CanvasGroup m_ArmorGroup;

	[Header("Party")]
	[SerializeField]
	private DamageColorSet m_Party;

	[Header("Ally")]
	[SerializeField]
	private DamageColorSet m_Ally;

	[Header("Enemy")]
	[SerializeField]
	private DamageColorSet m_Enemy;

	protected override void OnBind()
	{
		base.ViewModel.CurrentHp.CombineLatest(base.ViewModel.MaxHP, base.ViewModel.CurrentArmor, base.ViewModel.MaxArmor, base.ViewModel.IsDead, base.ViewModel.IsPlayer, base.ViewModel.IsEnemy, (int hp, int maxHP, int armor, int maxArmor, bool isDead, bool isPlayer, bool isEnemy) => new { hp, maxHP, armor, maxArmor, isDead, isPlayer, isEnemy }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(value =>
		{
			DamageColorSet damageColorSet = (value.isPlayer ? m_Party : (value.isEnemy ? m_Enemy : m_Ally));
			if (!value.isDead)
			{
				m_Health.color = Color.Lerp(((float)value.hp >= 0.5f) ? damageColorSet.DamageColor : damageColorSet.NearDeathColor, ((float)value.hp >= 0.5f) ? damageColorSet.NormalColor : damageColorSet.DamageColor, (float)value.hp);
			}
			else
			{
				m_Health.color = damageColorSet.NearDeathColor;
			}
			if (value.maxHP != 0)
			{
				m_Health.fillAmount = (float)value.hp / (float)value.maxHP;
				if (value.maxArmor > 0)
				{
					m_Armor.fillAmount = (float)value.armor / (float)value.maxArmor;
					m_ArmorGroup.alpha = 1f;
				}
				else
				{
					m_Armor.fillAmount = 0f;
					m_ArmorGroup.alpha = 0f;
				}
			}
		})
			.AddTo(this);
		this.SetTooltip(base.ViewModel.WoundsTooltip).AddTo(this);
	}

	public void SetVisible(bool visible)
	{
		m_MainGroup.alpha = (visible ? 1f : 0f);
	}
}
