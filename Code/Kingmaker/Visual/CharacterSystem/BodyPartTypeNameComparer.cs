using System.Collections.Generic;

namespace Kingmaker.Visual.CharacterSystem;

public class BodyPartTypeNameComparer : IComparer<BodyPartType>
{
	public static readonly BodyPartTypeNameComparer Instance = new BodyPartTypeNameComparer();

	public int Compare(BodyPartType x, BodyPartType y)
	{
		if ((object)x == y)
		{
			return 0;
		}
		if (x == null)
		{
			return -1;
		}
		if (y == null)
		{
			return 1;
		}
		return string.CompareOrdinal(x.Name, y.Name);
	}
}
