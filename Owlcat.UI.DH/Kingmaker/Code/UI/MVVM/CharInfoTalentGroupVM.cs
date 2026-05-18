using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Features;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoTalentGroupVM : ViewModel
{
	public readonly string Title;

	public readonly ObservableList<CharInfoTalentItemVM> TalentList = new ObservableList<CharInfoTalentItemVM>();

	public CharInfoTalentGroupVM(string title, List<BlueprintFeature> talents, ReadOnlyReactiveProperty<string> searchedString, MechanicEntity unit = null)
	{
		Title = title;
		foreach (BlueprintFeature talent in talents)
		{
			TalentList.Add(new CharInfoTalentItemVM(talent, searchedString, unit));
		}
	}
}
