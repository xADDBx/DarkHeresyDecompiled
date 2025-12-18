using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("9cec0bfbc21542f4a1690b6bfc8c3ab6")]
public abstract class ActionPointsChangedTrigger : UnitFactComponentDelegate
{
	protected enum PointsType
	{
		Yellow,
		Blue,
		Any
	}

	[SerializeField]
	protected PointsType m_Type;

	public RestrictionCalculator Restriction;

	public ActionList Actions;
}
