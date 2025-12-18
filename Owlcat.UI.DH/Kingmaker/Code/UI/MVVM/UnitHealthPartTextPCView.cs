using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class UnitHealthPartTextPCView : View<UnitHealthPartVM>
{
	[SerializeField]
	private TextMeshProUGUI m_HealthLabel;

	[SerializeField]
	private TextMeshProUGUI m_ArmorLabel;

	[SerializeField]
	private Color m_ArmorColor = Color.white;

	[SerializeField]
	private Color m_DeathColor;

	[SerializeField]
	private Color m_NormalColor;

	protected override void OnBind()
	{
		base.ViewModel.CurrentHp.CombineLatest(base.ViewModel.MaxHP, (int current, int max) => new { current, max }).Subscribe(value =>
		{
			if (!(m_HealthLabel == null))
			{
				m_HealthLabel.text = $"{value.current}";
				m_HealthLabel.color = (base.ViewModel.IsDead.CurrentValue ? m_DeathColor : m_NormalColor);
			}
		}).AddTo(this);
		base.ViewModel.CurrentArmor.CombineLatest(base.ViewModel.MaxArmor, (int current, int max) => new { current, max }).Subscribe(value =>
		{
			if (!(m_ArmorLabel == null))
			{
				if (value.max <= 0)
				{
					m_ArmorLabel.color = Color.clear;
				}
				else
				{
					m_ArmorLabel.text = $"{value.current}";
					m_ArmorLabel.color = m_ArmorColor;
				}
			}
		}).AddTo(this);
	}
}
