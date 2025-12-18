using System;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Tutorial.Solvers;

[Obsolete]
[ClassInfoBox("Always successful. All arguments should be gathered by trigger.")]
[TypeId("004d8198bc9c94a4f8dd4dff7035ed44")]
public class TutorialSolverAllFromTrigger : TutorialSolver
{
	public override bool Solve(TutorialContext context)
	{
		return true;
	}
}
