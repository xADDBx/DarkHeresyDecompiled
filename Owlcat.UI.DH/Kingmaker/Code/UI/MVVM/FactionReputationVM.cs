using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.MVVM.ServiceWindows;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class FactionReputationVM : ViewModel, IServiceWindow
{
	public readonly List<FactionReputationWidgetVM> FactionWidgetVMs = new List<FactionReputationWidgetVM>();

	private readonly ReactiveProperty<bool> m_SwitchedFromServiceWindow = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> SwitchedFromServiceWindow => m_SwitchedFromServiceWindow;

	public FactionReputationVM()
	{
		foreach (KeyValuePair<FactionType, BlueprintVendorFaction> item2 in (from i in ConfigRoot.Instance.SystemMechanics.VendorRoot.VendorFactions.Dereference().NotNull()
			group i by i.FactionType).ToDictionary((IGrouping<FactionType, BlueprintVendorFaction> g) => g.Key, (IGrouping<FactionType, BlueprintVendorFaction> g) => g.First()))
		{
			FactionReputationWidgetVM item = new FactionReputationWidgetVM(item2.Key, item2.Value);
			FactionWidgetVMs.Add(item);
		}
	}

	public void HandleOnSwitchedFromWindow()
	{
		m_SwitchedFromServiceWindow.Value = true;
	}
}
