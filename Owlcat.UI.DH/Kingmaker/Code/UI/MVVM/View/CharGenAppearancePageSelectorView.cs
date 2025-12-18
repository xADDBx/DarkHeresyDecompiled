using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenAppearancePageSelectorView : SelectionGroupRadioView<CharGenAppearancePageVM, CharGenAppearancePageMenuItemView>
{
	public List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return VirtualList.GetNavigationBehaviour().Entities.Select((IConsoleEntity e) => e as IConsoleNavigationEntity).ToList();
	}

	public IConsoleNavigationEntity GetSelectedEntity()
	{
		int index = VirtualList.GetNavigationBehaviour().Entities.Select((IConsoleEntity e) => (e as VirtualListElement)?.Data).FindIndex((IVirtualListElementData i) => (i as CharGenAppearancePageVM)?.IsSelected.Value ?? false);
		return VirtualList.GetNavigationBehaviour().Entities.ElementAt(index) as IConsoleNavigationEntity;
	}
}
