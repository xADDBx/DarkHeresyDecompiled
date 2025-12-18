using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class FactionReputationVM : ViewModel
{
	public readonly List<FactionReputationWidgetVM> FactionWidgetVMs = new List<FactionReputationWidgetVM>();

	public FactionReputationVM()
	{
		foreach (KeyValuePair<FactionType, BlueprintVendorFaction> item2 in (from i in ConfigRoot.Instance.SystemMechanics.VendorRoot.VendorFactions.Dereference().NotNull()
			group i by i.FactionType).ToDictionary((IGrouping<FactionType, BlueprintVendorFaction> g) => g.Key, (IGrouping<FactionType, BlueprintVendorFaction> g) => g.First()))
		{
			FactionReputationWidgetVM item = new FactionReputationWidgetVM(item2.Key, item2.Value);
			FactionWidgetVMs.Add(item);
		}
	}
}
