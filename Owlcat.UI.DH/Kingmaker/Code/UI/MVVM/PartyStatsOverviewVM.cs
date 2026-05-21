using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class PartyStatsOverviewVM : ViewModel
{
	private readonly ReactiveProperty<bool> m_IsActive = new ReactiveProperty<bool>(value: false);

	public readonly ObservableList<PartyStatsOverviewCharacterVM> Items = new ObservableList<PartyStatsOverviewCharacterVM>();

	public ReadOnlyReactiveProperty<bool> IsActive => m_IsActive;

	public void ShowForStat(StatType stat, BaseUnitEntity excludedUnit)
	{
		ClearItems();
		if (Game.Instance.Player != null)
		{
			(from ch in UtilityParty.GetGroup(withRemote: true)
				where ch != excludedUnit
				select ch into u
				select new PartyStatsOverviewCharacterVM(u, stat) into vm
				orderby vm.StatValue descending
				select vm).ForEach(Items.Add);
		}
		m_IsActive.Value = true;
	}

	public void Hide()
	{
		ClearItems();
		m_IsActive.Value = false;
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		ClearItems();
	}

	private void ClearItems()
	{
		foreach (PartyStatsOverviewCharacterVM item in Items)
		{
			item.Dispose();
		}
		Items.Clear();
	}
}
