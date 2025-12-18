using Kingmaker.Blueprints;
using Kingmaker.Gameplay.Features.Reputation;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FactionReputationWidgetVM : ViewModel
{
	public readonly FactionType FactionType;

	public readonly BlueprintVendorFaction VendorFaction;

	private readonly ReactiveProperty<int> m_FearLevel = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_RespectLevel = new ReactiveProperty<int>();

	public ReadOnlyReactiveProperty<int> FearLevel => m_FearLevel;

	public ReadOnlyReactiveProperty<int> RespectLevel => m_RespectLevel;

	public FactionReputationWidgetVM(FactionType factionType, BlueprintVendorFaction blueprint)
	{
		FactionType = factionType;
		VendorFaction = blueprint;
		UpdateReputation();
	}

	public void UpdateReputation()
	{
		var (value, value2) = (ReputationDescription)(ref ReputationHelper.GetReputation(FactionType));
		m_FearLevel.Value = Mathf.Clamp(value, 0, 100);
		m_RespectLevel.Value = Mathf.Clamp(value2, 0, 100);
	}

	protected override void OnDispose()
	{
		FearLevel.Dispose();
		RespectLevel.Dispose();
	}
}
