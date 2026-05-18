using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class AbilityAppliedModifierView : View<AbilityAppliedModifierVM>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private GameObject m_LockMarker;

	[SerializeField]
	private TMP_Text m_DescriptionText;

	[SerializeField]
	private MonoBehaviour m_TooltipSource;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		m_Icon.sprite = base.ViewModel.Icon;
		m_Background.color = base.ViewModel.BackgroundColor;
		m_LockMarker.SetActive(base.ViewModel.IsLocked);
		m_TextHelper = new AccessibilityTextHelper(m_DescriptionText).AddTo(this);
		m_DescriptionText.SetText(base.ViewModel.Description);
		m_TextHelper.UpdateTextSize();
		if (base.ViewModel.Tooltip != null)
		{
			m_TooltipSource.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		}
	}
}
