using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.Localization;

namespace Kingmaker.Code.Gameplay.Blueprints.Root.Strings;

public class PowerBalanceStrings : StringsContainer
{
	public LocalizedString PowerBalanceHeader;

	public LocalizedString PlayerHeader;

	public LocalizedString EnemyHeader;

	public LocalizedString CurrentLabel;

	public LocalizedString LosingBattleLabel;

	public LocalizedString ShatteredLabel;

	public LocalizedString LosingBattleDescription;

	public LocalizedString ShatteredDescription;

	public LocalizedString PowerBalanceStateLabel;

	public LocalizedString StateRegular;

	public LocalizedString StateLosingBattle;

	public LocalizedString StateShattered;

	public LocalizedString GroupChangedStateMessage;

	public LocalizedString GroupChangedStateGenericMessage;

	public string GetStateLocalized(MoraleGroup moraleGroup)
	{
		return moraleGroup.PowerBalanceState switch
		{
			PowerBalanceState.Regular => StateRegular, 
			PowerBalanceState.LosingBattle => StateLosingBattle, 
			PowerBalanceState.Shattered => StateShattered, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
