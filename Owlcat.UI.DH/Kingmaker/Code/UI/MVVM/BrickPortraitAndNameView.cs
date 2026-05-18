using Code.View.UI.Helpers;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickPortraitAndNameView : BrickBaseView<BrickPortraitAndNameVM>
{
	[Header("Elements")]
	[SerializeField]
	protected BrickTitleView BrickTitleView;

	[SerializeField]
	protected TMP_Text m_Title;

	[SerializeField]
	protected TMP_Text m_DifficultyText;

	[SerializeField]
	protected Image m_Icon;

	[Header("Selectables")]
	[SerializeField]
	protected OwlcatMultiSelectable m_IconSelectable;

	[SerializeField]
	protected OwlcatMultiSelectable m_BorderAndDifficultySelectable;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title, m_DifficultyText).AddTo(this);
		}
		base.OnBind();
		m_Icon.sprite = base.ViewModel.Icon;
		m_Title.text = base.ViewModel.Line;
		BrickTitleView.Bind(base.ViewModel.BrickTitle);
		BrickTitleView.gameObject.SetActive(base.ViewModel.BrickTitle != null);
		m_BorderAndDifficultySelectable.SetActiveLayer(base.ViewModel.PortraitType.ToString());
		m_IconSelectable.SetActiveLayer(base.ViewModel.IsUsedSubtypeIcon ? base.ViewModel.PortraitType.ToString() : "Default");
		m_DifficultyText.gameObject.SetActive(base.ViewModel.Difficulty > 0);
		m_DifficultyText.text = UIUtilityText.ArabicToRoman(base.ViewModel.Difficulty);
		m_TextHelper.UpdateTextSize();
	}
}
