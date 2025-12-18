using System;
using System.Text;
using Framework.Utility.DotNetExtensions;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("1e2e8303aab44b3488ef09fe382fb1e4")]
public sealed class CloseCase : GameAction
{
	[ValidateNotNull]
	public BpRef<BlueprintCase> Case = new BpRef<BlueprintCase>();

	public bool WithAnswer;

	[ShowIf("WithAnswer")]
	public BpRef<BlueprintCaseAnswer> Answer = new BpRef<BlueprintCaseAnswer>();

	public override string GetCaption()
	{
		StringBuilder value;
		using (StringBuilderPool.Get(out value))
		{
			value.Append("Close case ");
			value.Append(Case);
			if (WithAnswer)
			{
				value.Append(" with answer ");
				value.Append(Answer);
			}
			return value.ToString();
		}
	}

	protected override void RunAction()
	{
		if (WithAnswer)
		{
			Game.Instance.DetectiveSystem.CloseCase(Case, Answer);
		}
		else
		{
			Game.Instance.DetectiveSystem.CloseCaseWithoutAnswer(Case);
		}
	}
}
