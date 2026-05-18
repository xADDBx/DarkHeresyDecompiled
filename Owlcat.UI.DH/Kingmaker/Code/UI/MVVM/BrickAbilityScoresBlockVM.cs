using Kingmaker.EntitySystem.Entities;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityScoresBlockVM : TooltipBrickVM
{
	public readonly CharInfoAbilityScoresBlockVM AbilityScoresBlock;

	public BrickAbilityScoresBlockVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
	{
		AbilityScoresBlock = new CharInfoAbilityScoresBlockVM(unit).AddTo(this);
	}
}
