using System;
using System.Collections.Generic;

namespace Owlcat.UI;

[Serializable]
public class IconsByAction
{
	public RewiredActionType Type;

	public List<SpriteByConsole> Icons;
}
