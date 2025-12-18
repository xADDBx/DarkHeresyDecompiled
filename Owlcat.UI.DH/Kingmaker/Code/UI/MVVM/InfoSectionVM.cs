using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InfoSectionVM : ViewModel
{
	private readonly ReactiveProperty<TooltipBaseTemplate> m_TooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<InfoBodyVM> m_InfoVM = new ReactiveProperty<InfoBodyVM>();

	public ReadOnlyReactiveProperty<InfoBodyVM> InfoVM => m_InfoVM;

	public TooltipBaseTemplate CurrentTooltip => m_TooltipTemplate.Value;

	public InfoSectionVM()
	{
		m_TooltipTemplate.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate(TooltipBaseTemplate temp)
		{
			SetTemplate(temp);
		}).AddTo(this);
	}

	protected override void OnDispose()
	{
		m_InfoVM.Value?.Dispose();
	}

	public void SetTemplate(TooltipBaseTemplate template)
	{
		m_TooltipTemplate.Value = template;
		m_InfoVM.Value?.Dispose();
		m_InfoVM.Value = ((template != null) ? new InfoBodyVM(template) : null);
	}
}
