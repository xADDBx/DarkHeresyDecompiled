using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Solvers;

[Obsolete]
[TypeId("fa121bf109a094d4095c1cf4eac6a1fb")]
public class TutorialSolverItemFromTrigger : TutorialSolver
{
	public override bool Solve(TutorialContext context)
	{
		return context.SolutionItem != null;
	}
}
