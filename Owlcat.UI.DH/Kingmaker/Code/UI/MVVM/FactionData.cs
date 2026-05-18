using Kingmaker.Gameplay.Features.Reputation;

namespace Kingmaker.Code.UI.MVVM;

public class FactionData
{
	public FactionType FactionType { get; }

	public ReputationType ReputationType { get; }

	public int Count { get; private set; }

	public FactionData(FactionType factionType, ReputationType reputationType, int count)
	{
		FactionType = factionType;
		ReputationType = reputationType;
		Count = count;
	}

	public void AddCount(int value)
	{
		Count += value;
	}
}
