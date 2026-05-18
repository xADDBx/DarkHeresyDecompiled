using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatUnitCounterView : View<CombatUnitCounterVM>
{
	[Header("Enemies Counter")]
	[SerializeField]
	private TMP_Text m_EnemiesCountLabel;

	[SerializeField]
	private MonoBehaviour m_EnemiesCountHintSource;

	[Header("Allies Counter")]
	[SerializeField]
	private TMP_Text m_AlliesCountLabel;

	[SerializeField]
	private MonoBehaviour m_AlliesCountHintSource;

	protected override void OnBind()
	{
		base.ViewModel.EnemiesCount.Subscribe(delegate(int count)
		{
			m_EnemiesCountLabel.SetText(count.ToString());
		}).AddTo(this);
		base.ViewModel.AlliesCount.Subscribe(delegate(int count)
		{
			m_AlliesCountLabel.SetText(count.ToString());
		}).AddTo(this);
		m_EnemiesCountHintSource.SetHint(UIStrings.Instance.HUDTexts.EnemiesInCombat).AddTo(this);
		m_AlliesCountHintSource.SetHint(UIStrings.Instance.HUDTexts.AlliesInCombat).AddTo(this);
	}

	protected override void OnUnbind()
	{
	}
}
