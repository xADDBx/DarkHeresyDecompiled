using Kingmaker.Blueprints;
using Kingmaker.Enums;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenPortraitGroupVM : ViewModel
{
	private readonly ReactiveProperty<bool> m_Expanded = new ReactiveProperty<bool>();

	public readonly PortraitCategory PortraitCategory;

	public readonly ObservableList<CharGenPortraitSelectorItemVM> PortraitCollection = new ObservableList<CharGenPortraitSelectorItemVM>();

	public ReadOnlyReactiveProperty<bool> Expanded => m_Expanded;

	public CharGenPortraitGroupVM()
	{
	}

	public CharGenPortraitGroupVM(PortraitCategory portraitCategory)
	{
		PortraitCategory = portraitCategory;
	}

	public void Add(CharGenPortraitSelectorItemVM portrait)
	{
		PortraitCollection.Add(portrait);
	}

	public void RemoveNonexistentItems()
	{
		PortraitCollection.RemoveAll(delegate(CharGenPortraitSelectorItemVM item)
		{
			PortraitData portraitData = item.PortraitData;
			return (portraitData == null) ? (!item.CustomPortraitCreatorItem) : (!portraitData.EnsureImages());
		});
	}

	public void RemoveCustomItems()
	{
		PortraitCollection.RemoveAll((CharGenPortraitSelectorItemVM item) => item.PortraitData?.IsCustom ?? (!item.CustomPortraitCreatorItem));
	}

	public void Clear()
	{
		PortraitCollection.Clear();
	}

	public CharGenPortraitSelectorItemVM GetByIdOrFirstValid(string id)
	{
		return PortraitCollection.FirstOrDefault((CharGenPortraitSelectorItemVM i) => i.PortraitData?.CustomId == id) ?? PortraitCollection.FirstOrDefault((CharGenPortraitSelectorItemVM i) => !i.CustomPortraitCreatorItem);
	}

	public void SetExpanded(bool isExpanded)
	{
		m_Expanded.Value = isExpanded;
	}
}
