using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickBuffDOTView : BrickBaseView<BrickBuffDOTVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private TMP_Text m_DamageText;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title, m_Description, m_DamageText).AddTo(this);
		}
		base.OnBind();
		m_Title.SetText(UIStrings.Instance.Tooltips.Damage.Text);
		m_Description.SetText(UIStrings.Instance.Tooltips.EveryRound.Text);
		m_DamageText.SetText(base.ViewModel.Damage);
		m_TextHelper.UpdateTextSize();
	}
}
