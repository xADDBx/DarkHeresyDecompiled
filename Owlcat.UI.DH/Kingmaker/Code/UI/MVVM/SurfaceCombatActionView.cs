using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SurfaceCombatActionView : View<SurfaceCombatActionVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TooltipConfig m_TooltipConfig;

	protected override void OnBind()
	{
		bool flag = base.ViewModel.ActionAbility != null;
		SetVisible(flag);
		if (flag)
		{
			m_Icon.sprite = base.ViewModel.Icon;
			m_Icon.raycastTarget = base.ViewModel.ActionAbility != null;
			m_Icon.SetTooltip(base.ViewModel.ActionAbilityTooltip, m_TooltipConfig);
			m_Icon.SetHint(UIStrings.Instance.HUDTexts.ConcentrationHint);
		}
	}

	protected override void OnUnbind()
	{
		SetVisible(visible: false);
	}

	private void SetVisible(bool visible)
	{
		base.gameObject.SetActive(visible);
		if (!visible)
		{
			m_Icon.sprite = null;
			return;
		}
		m_CanvasGroup.alpha = 1f;
		m_Icon.raycastTarget = true;
	}
}
