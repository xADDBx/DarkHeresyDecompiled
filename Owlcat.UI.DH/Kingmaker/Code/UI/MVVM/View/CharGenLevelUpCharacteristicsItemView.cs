using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenLevelUpCharacteristicsItemView : CharGenLevelUpSelectorBaseItemView<CharGenLevelUpCharacteristicsItemVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private TextMeshProUGUI m_AddedValue;

	[SerializeField]
	private GameObject m_BattleSkillMark;

	[SerializeField]
	private GameObject m_AttributeMark;

	[SerializeField]
	private OwlcatMultiSelectable m_BonusPenaltyState;

	[SerializeField]
	private TextMeshProUGUI m_MaxText;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_BattleSkillMark != null)
		{
			m_BattleSkillMark.SetActive(base.ViewModel.IsBattleStat);
		}
		if (m_Value != null)
		{
			AddDisposable(base.ViewModel.Points.Subscribe(delegate(int p)
			{
				m_Value.text = p.ToString();
			}));
		}
		if (m_AddedValue != null)
		{
			AddDisposable(base.ViewModel.AddedPoints.Subscribe(delegate(int p)
			{
				m_AddedValue.text = "+" + p;
			}));
		}
		if (m_AttributeMark != null)
		{
			m_AttributeMark.SetActive(base.ViewModel.HasAttributeMark);
		}
		if (m_BonusPenaltyState != null)
		{
			m_BonusPenaltyState.SetActiveLayer(base.ViewModel.BonusPenaltyState.ToString());
		}
		if (m_MaxText != null)
		{
			m_MaxText.text = UIStrings.Instance.CharGen.SkillMaxValue;
		}
	}
}
