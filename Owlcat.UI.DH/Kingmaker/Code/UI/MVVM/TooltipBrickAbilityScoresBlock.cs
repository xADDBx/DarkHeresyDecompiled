using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickAbilityScoresBlock : ITooltipBrick
{
	private readonly CharInfoAbilityScoresBlockVM m_AbilityScoresBlock;

	private readonly ReactiveCommand<Unit> m_CharacteristicsChanged;

	public TooltipBrickAbilityScoresBlock(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
	{
		m_AbilityScoresBlock = new CharInfoAbilityScoresBlockVM(unit);
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickAbilityScoresBlockVM(m_AbilityScoresBlock);
	}
}
