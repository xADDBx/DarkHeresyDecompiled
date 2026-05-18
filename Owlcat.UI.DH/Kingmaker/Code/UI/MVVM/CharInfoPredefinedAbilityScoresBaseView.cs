using System.Collections.Generic;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoPredefinedAbilityScoresBaseView : CharInfoComponentWithLevelUpView<CharInfoAbilityScoresBlockVM>
{
	[SerializeField]
	private List<View<CharInfoStatVM>> m_StatEntries;

	protected override void OnBind()
	{
		base.OnBind();
		BindEntries();
	}

	private void BindEntries()
	{
		AutoDisposingList<CharInfoStatVM> stats = base.ViewModel.Stats;
		for (int i = 0; i < m_StatEntries.Count; i++)
		{
			if (i < stats.Count)
			{
				m_StatEntries[i].Bind(stats[i]);
			}
			m_StatEntries[i].gameObject.SetActive(i < stats.Count);
		}
	}
}
