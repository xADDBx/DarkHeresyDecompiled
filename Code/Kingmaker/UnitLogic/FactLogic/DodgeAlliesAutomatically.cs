using System;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete]
[TypeId("0bd3b0417d194307b76dfa6a66522267")]
public class DodgeAlliesAutomatically : BlueprintComponent
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();
}
