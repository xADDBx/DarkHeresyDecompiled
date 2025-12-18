using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete]
[TypeId("35018634ecb04c539db2c88a5cd2ab44")]
public class RandomizeScatterShotDirections : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public ContextValue Main = new ContextValue
	{
		Value = 20
	};

	public ContextValue Near = new ContextValue
	{
		Value = 40
	};

	public ContextValue Far = new ContextValue
	{
		Value = 40
	};
}
