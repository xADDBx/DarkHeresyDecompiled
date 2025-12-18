using Kingmaker.Gameplay.Features.Reputation;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VendorReputationLevelVM : VirtualListElementVMBase
{
	public int ReputationLevel;

	public bool Locked;

	public int ReputationPoints;

	public int NextLevelReputationPoints;

	public int CurrentReputationPoints;

	public int Delta;

	public int Difference;

	private readonly ReactiveCommand<Unit> m_OnHighlight = new ReactiveCommand<Unit>();

	public Observable<Unit> OnHighlight => m_OnHighlight;

	public VendorReputationLevelVM(int level, bool locked)
	{
		ReputationLevel = level;
		Locked = locked;
		if (!Locked)
		{
			Delta = 1;
			Difference = 1;
			return;
		}
		int currentReputationLevel = ReputationHelper.GetCurrentReputationLevel(Game.Instance.TradeLogic.VendorFaction.FactionType);
		if ((ReputationHelper.GetNextLvl(Game.Instance.TradeLogic.VendorFaction.FactionType) ?? level) == level)
		{
			ReputationPoints = ReputationHelper.GetReputationPointsByLevel(Game.Instance.TradeLogic.VendorFaction.FactionType, currentReputationLevel);
			NextLevelReputationPoints = ReputationHelper.GetReputationPointsByLevel(Game.Instance.TradeLogic.VendorFaction.FactionType, level);
		}
		else
		{
			ReputationPoints = ReputationHelper.GetReputationPointsByLevel(Game.Instance.TradeLogic.VendorFaction.FactionType, level);
			NextLevelReputationPoints = ReputationHelper.GetReputationPointsByLevel(Game.Instance.TradeLogic.VendorFaction.FactionType, level + 1);
		}
		CurrentReputationPoints = ReputationHelper.GetCurrentReputationPoints(Game.Instance.TradeLogic.VendorFaction.FactionType);
		Delta = NextLevelReputationPoints - ReputationPoints;
		Difference = CurrentReputationPoints - ReputationPoints;
	}

	protected override void DisposeImplementation()
	{
	}
}
