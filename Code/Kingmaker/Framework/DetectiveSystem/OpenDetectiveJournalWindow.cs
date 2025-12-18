using System;
using System.Text;
using Framework.Utility.DotNetExtensions;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("3338db28566d4994a8a76bfe4f39af47")]
public sealed class OpenDetectiveJournalWindow : GameAction
{
	public BpRef<BlueprintCase> Case = new BpRef<BlueprintCase>();

	[ShowIf("IsSpecificCase")]
	public bool RequireReportForCloseWindow;

	private bool IsSpecificCase => !Case.IsNull();

	public override string GetCaption()
	{
		StringBuilder value;
		using (StringBuilderPool.Get(out value))
		{
			value.Append("Open detective journal window");
			if (IsSpecificCase)
			{
				value.Append(" for case ");
				value.Append(Case);
				if (RequireReportForCloseWindow)
				{
					value.Append(" (require report)");
				}
			}
			return value.ToString();
		}
	}

	protected override void RunAction()
	{
		EventBus.RaiseEvent(delegate(IDetectiveJournalUIHandler h)
		{
			h.HandleOpenDetectiveJournal(Case, null, IsSpecificCase && RequireReportForCloseWindow);
		});
	}
}
