using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Pathfinding;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete]
[ComponentName("Damage/ReceiveDamageInsteadOfCaster")]
[TypeId("7d07d6649d9d471c82592b2b50405df1")]
public class ReceiveDamageInsteadOfCaster : UnitFactComponentDelegate
{
	public int MaxDistanceToCaster;

	private Cells MaxDistanceToCasterCells => MaxDistanceToCaster.Cells();
}
