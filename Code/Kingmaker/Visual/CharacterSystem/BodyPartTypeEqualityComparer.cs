using System.Collections.Generic;

namespace Kingmaker.Visual.CharacterSystem;

public class BodyPartTypeEqualityComparer : IEqualityComparer<BodyPartType>
{
	public bool Equals(BodyPartType x, BodyPartType y)
	{
		if (!(x == y))
		{
			return x?.Name == y?.Name;
		}
		return true;
	}

	public int GetHashCode(BodyPartType obj)
	{
		return obj.GetHashCode();
	}
}
