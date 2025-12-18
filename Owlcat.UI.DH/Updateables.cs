using System;
using System.Collections.Generic;
using Kingmaker.UI.Selection.UnitMark;

public static class Updateables
{
	public static readonly List<Type> IUpdateables = new List<Type>();

	public static readonly List<Type> ILateUpdateables = new List<Type>
	{
		typeof(BaseSurfaceUnitMark),
		typeof(BaseUnitMark),
		typeof(CharacterUnitMark),
		typeof(EnemyUnitMark),
		typeof(NpcUnitMark)
	};
}
