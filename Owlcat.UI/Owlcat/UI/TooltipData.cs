using R3;

namespace Owlcat.UI;

public class TooltipData
{
	private readonly TooltipBaseTemplate m_Template;

	public readonly TooltipConfig Config;

	public readonly ReactiveCommand<Unit> CloseCommand;

	public ConsoleNavigationBehaviour OwnerNavigationBehaviour;

	public TooltipBaseTemplate MainTemplate => m_Template;

	public TooltipData(TooltipBaseTemplate template, TooltipConfig config, ReactiveCommand<Unit> closeCommand = null, ConsoleNavigationBehaviour ownerNavigationBehaviour = null)
	{
		m_Template = template;
		Config = config;
		CloseCommand = closeCommand;
		OwnerNavigationBehaviour = ownerNavigationBehaviour;
	}
}
