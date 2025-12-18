using Kingmaker.Blueprints.Encyclopedia;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickHistoryManagement : ITooltipBrick
{
	private readonly BlueprintEncyclopediaGlossaryEntry m_GlossaryEntry;

	private readonly BlueprintEncyclopediaEntry m_EncyclopediaEntry;

	public TooltipBrickHistoryManagement(BlueprintEncyclopediaGlossaryEntry glossaryEntry, BlueprintEncyclopediaEntry encyclopediaEntry)
	{
		m_GlossaryEntry = glossaryEntry;
		m_EncyclopediaEntry = encyclopediaEntry;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickHistoryManagementVM(m_GlossaryEntry, m_EncyclopediaEntry);
	}
}
