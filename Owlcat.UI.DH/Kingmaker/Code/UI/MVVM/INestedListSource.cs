using System.Collections.Generic;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public interface INestedListSource
{
	INestedListSource Source { get; }

	bool HasNesting { get; }

	List<NestedSelectionGroupEntityVM> ExtractNestedEntities();

	ReactiveProperty<NestedSelectionGroupEntityVM> GetSelectedReactiveProperty();
}
