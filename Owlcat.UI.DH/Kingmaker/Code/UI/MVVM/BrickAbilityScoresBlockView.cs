using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityScoresBlockView : BrickBaseView<BrickAbilityScoresBlockVM>
{
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	protected CharInfoAbilityScoresBlockBaseView m_AbilityScoresBlockView;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title);
		}
		base.OnBind();
		m_Title.text = UIStrings.Instance.Inspect.CharacterStatsTitle.Text;
		m_AbilityScoresBlockView.Bind(base.ViewModel.AbilityScoresBlock);
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_TextHelper.Dispose();
	}
}
