using Kingmaker.ElementsSystem;
using Kingmaker.Framework;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("cfd45b6275ae2234cb9b5d7d4d10c02e")]
public abstract class ContextCondition : Condition
{
	protected EvalContext Eval => EvalContext.Current;

	protected TargetWrapper Target => Eval.Target;
}
