using Code.Framework.Utility.UnityExtensions;
using Code.View.UI.Helpers;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class BrickCombatLogBaseView<T> : BrickBaseView<T> where T : BrickCombatLogBaseVM
{
	[Header("Elements")]
	[SerializeField]
	protected TMP_Text m_NameText;

	[SerializeField]
	protected Image m_IconImage;

	[SerializeField]
	protected TMP_Text m_ResultValueText;

	[SerializeField]
	protected GameObject m_ResultLineImage;

	[Header("Selectables")]
	[SerializeField]
	protected OwlcatMultiSelectable m_BackgroundStateSelectable;

	[SerializeField]
	protected OwlcatMultiSelectable m_NestedSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_SpriteSelectable;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_NameText, m_ResultValueText);
		}
		m_NameText.text = base.ViewModel.Name;
		if (base.ViewModel.Tooltip != null)
		{
			TMP_Text nameText = m_NameText;
			nameText.text = nameText.text + " " + UIUtilityText.GetQuestionSprite();
		}
		m_NestedSelectable.SetActiveLayer(base.ViewModel.NestedLevel);
		m_SpriteSelectable.SetActiveLayer(base.ViewModel.IconType.ToString());
		m_IconImage.gameObject.SetActive(base.ViewModel.IconType != CombatLogIcon.None);
		m_BackgroundStateSelectable.SetActiveLayer(base.ViewModel.Palette.ToString());
		m_ResultValueText.text = base.ViewModel.ResultValue;
		m_ResultValueText.gameObject.SetActive(base.ViewModel.IsResultValue && !base.ViewModel.ResultValue.IsNullOrEmpty());
		m_ResultLineImage.SetActive(base.ViewModel.IsResultValue && base.ViewModel.ResultValue.IsNullOrEmpty());
		m_BackgroundStateSelectable.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}
}
