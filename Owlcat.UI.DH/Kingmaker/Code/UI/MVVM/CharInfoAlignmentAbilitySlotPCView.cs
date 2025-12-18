using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAlignmentAbilitySlotPCView : CharInfoComponentView<CharInfoAlignmentAbilitySlotVM>
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private Image m_ProgressImage;

	private TooltipHandler m_TooltipHandler;

	public SimpleConsoleNavigationEntity NavigationEntity { get; private set; }

	protected override void OnBind()
	{
		base.OnBind();
		NavigationEntity = new SimpleConsoleNavigationEntity(m_Button, base.ViewModel.Tooltip);
	}

	protected override void RefreshView()
	{
		m_Button.SetActiveLayer(base.ViewModel.CurrentSlotState.ToString());
		m_TooltipHandler?.Dispose();
		m_TooltipHandler = this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		m_ProgressImage.fillAmount = base.ViewModel.Progress;
		NavigationEntity = new SimpleConsoleNavigationEntity(m_Button, base.ViewModel.Tooltip);
	}
}
