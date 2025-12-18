using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class VeilBuffView : View<IUIDataProviderVM>
{
	[SerializeField]
	private Image m_Icon;

	protected override void OnBind()
	{
		m_Icon.sprite = base.ViewModel.Icon;
		this.SetTooltip(new TooltipTemplateSimple(base.ViewModel.Name, base.ViewModel.Description), new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.None)).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_Icon.sprite = null;
	}
}
