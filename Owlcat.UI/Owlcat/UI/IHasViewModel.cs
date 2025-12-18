using System;

namespace Owlcat.UI;

[Obsolete]
public interface IHasViewModel
{
	IViewModel GetViewModel();
}
