using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers.Units;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct MovableEntitiesEnumerable
{
	public List<Kingmaker.Mechanics.Entities.AbstractUnitEntity>.Enumerator GetEnumerator()
	{
		return Game.Instance.EntityPools.AllAwakeUnits.ToTempList().GetEnumerator();
	}
}
