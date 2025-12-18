using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("603b5218c76141dd8dcae6e3f4a52162")]
public class ShowMessageBox : GameAction
{
	public LocalizedString Text;

	public ActionList OnClose;

	public int WaitTime;

	public override string GetCaption()
	{
		return "Show message box";
	}

	protected override void RunAction()
	{
		UtilityMessageBox.ShowMessageBox(Text, DialogMessageBoxType.Message, delegate
		{
			OnClose.Run();
		}, null, null, null, WaitTime);
	}
}
