using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAlignmentAbilitySlotBaseView : CharInfoComponentView<CharInfoAlignmentAbilitySlotVM>
{
	[Header("Elements")]
	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[SerializeField]
	private Image m_ProgressImage;

	private TooltipHandler m_TooltipHandler;

	protected override void RefreshView()
	{
		base.ViewModel.CurrentSlotState.Subscribe(delegate(CharInfoAlignmentAbilitySlotVM.SlotState value)
		{
			m_Button.SetActiveLayer(value.ToString());
		}).AddTo(this);
		base.ViewModel.Progress.Subscribe(delegate(float value)
		{
			m_ProgressImage.fillAmount = value;
		}).AddTo(this);
		base.ViewModel.Tooltip.Subscribe(delegate(TooltipBaseTemplate value)
		{
			m_TooltipHandler?.Dispose();
			m_TooltipHandler = this.SetTooltip(value).AddTo(this);
		}).AddTo(this);
	}
}
