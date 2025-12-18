using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Reputation;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFactionsReputationVM : CharInfoComponentVM
{
	public readonly ObservableList<ViewModel> ScreenItems = new ObservableList<ViewModel>();

	public CharInfoFactionsReputationVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		foreach (FactionType value in Enum.GetValues(typeof(FactionType)))
		{
			if (value != 0)
			{
				ScreenItems.Add(new CharInfoFactionReputationItemVM(value));
			}
		}
		Disposable.Create(DisposeImplementation).AddTo(this);
	}

	private void DisposeImplementation()
	{
		ScreenItems.ForEach(delegate(ViewModel vm)
		{
			vm.Dispose();
		});
		ScreenItems.Clear();
	}
}
