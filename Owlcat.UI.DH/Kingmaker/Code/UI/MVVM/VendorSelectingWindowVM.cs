using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Reputation;
using ObservableCollections;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class VendorSelectingWindowVM : ViewModel
{
	public readonly ObservableList<CharInfoFactionReputationItemVM> FactionItems = new ObservableList<CharInfoFactionReputationItemVM>();

	public VendorSelectingWindowVM([CanBeNull] List<MechanicEntity> vendors)
	{
		foreach (FactionType value in Enum.GetValues(typeof(FactionType)))
		{
			if (value != 0)
			{
				FactionItems.Add(new CharInfoFactionReputationItemVM(value, canTrade: true, vendors));
			}
		}
	}

	protected override void OnDispose()
	{
		FactionItems.ForEach(delegate(CharInfoFactionReputationItemVM vm)
		{
			vm.Dispose();
		});
		FactionItems.Clear();
	}
}
