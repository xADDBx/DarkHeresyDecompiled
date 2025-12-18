using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarSlotConsoleView : View<ActionBarSlotVM>, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplate
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private CanvasSortingComponent m_CanvasSortingComponent;

	[Header("Convert Block")]
	[SerializeField]
	private ActionBarConvertedConsoleView m_ConvertedView;

	[SerializeField]
	private bool m_ShowConvertButton;

	[SerializeField]
	[ShowIf("m_ShowConvertButton")]
	private GameObject m_ConvertButton;

	private RectTransform m_RectTransform;

	private CanvasGroup m_CanvasGroup;

	public ActionBarConvertedConsoleView ConvertedView => m_ConvertedView;

	protected override void OnBind()
	{
		if ((bool)m_CanvasSortingComponent)
		{
			m_CanvasSortingComponent.PushView().AddTo(this);
		}
		if ((bool)m_ConvertedView)
		{
			base.ViewModel.HasConvert.And(base.ViewModel.IsPossibleActive).Subscribe(m_ConvertedView.gameObject.SetActive).AddTo(this);
			base.ViewModel.ConvertedVm.Skip(1).Subscribe(delegate(ActionBarConvertedVM vms)
			{
				SetFocus(vms == null);
				m_ConvertedView.Bind(vms);
			}).AddTo(this);
			base.ViewModel.HasConvert.Subscribe(delegate(bool value)
			{
				m_ConvertButton.Or(null)?.SetActive(value && m_ShowConvertButton);
			}).AddTo(this);
		}
	}

	protected override void OnUnbind()
	{
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
		if (value)
		{
			base.ViewModel?.OnHoverOn();
		}
		else
		{
			base.ViewModel?.OnHoverOff();
		}
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}

	public bool CanConfirmClick()
	{
		return !base.ViewModel.IsEmpty.CurrentValue;
	}

	public void OnConfirmClick()
	{
		base.ViewModel.OnMainClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.CurrentValue;
	}

	public void ShowConvertRequest()
	{
		base.ViewModel.OnShowConvertRequest();
	}
}
