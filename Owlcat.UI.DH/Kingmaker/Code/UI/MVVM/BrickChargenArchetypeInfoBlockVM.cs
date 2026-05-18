using System.Collections.Generic;

namespace Kingmaker.Code.UI.MVVM;

public class BrickChargenArchetypeInfoBlockVM : TooltipBrickVM
{
	public readonly string Description;

	public readonly List<float> SchemeData;

	public BrickChargenArchetypeInfoBlockVM(string description, List<float> schemeData)
	{
		Description = description;
		SchemeData = schemeData;
	}
}
