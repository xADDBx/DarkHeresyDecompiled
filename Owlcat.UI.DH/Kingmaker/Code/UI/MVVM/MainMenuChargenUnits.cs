using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.DLC;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class MainMenuChargenUnits : IDisposable
{
	public static MainMenuChargenUnits Instance;

	private readonly ReactiveProperty<ChargenUnit> m_CurrentPregenUnit = new ReactiveProperty<ChargenUnit>();

	public BlueprintDlcRewardCampaign DlcReward;

	public ReadOnlyReactiveProperty<ChargenUnit> CurrentPregenUnit => m_CurrentPregenUnit;

	public BlueprintPortrait CustomCharacterPortrait => ConfigRoot.Instance.CharGenRoot.CustomPortrait;

	public MainMenuChargenUnits()
	{
		Instance = this;
		ConfigRoot.Instance.CharGenRoot.EnsureNewGamePregens(null);
		ConfigRoot.Instance.CharGenRoot.EnsureShipPregens(null);
	}

	public void Dispose()
	{
		Instance = null;
		CurrentPregenUnit?.Dispose();
		ConfigRoot.Instance.CharGenRoot.DisposeUnitsForChargen();
	}
}
