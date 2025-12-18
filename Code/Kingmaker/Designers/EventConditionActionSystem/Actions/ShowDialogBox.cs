using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("09a180a958e44f641b9990d0f96aeed4")]
public class ShowDialogBox : GameAction
{
	public LocalizedString Text;

	public ParametrizedContextSetter Parameters;

	public ActionList OnAccept;

	public ActionList OnCancel;

	public override string GetCaption()
	{
		return "Show dialog box " + Text;
	}

	protected override void RunAction()
	{
		NamedParametersContext parameters = null;
		ParametrizedContextSetter.ParameterEntry[] array = (Parameters?.Parameters).EmptyIfNull();
		foreach (ParametrizedContextSetter.ParameterEntry parameterEntry in array)
		{
			parameters = parameters ?? new NamedParametersContext();
			parameters.Params[parameterEntry.Name] = parameterEntry.GetValue();
		}
		UtilityMessageBox.ShowMessageBox(Text, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
		{
			using (parameters?.RequestContextData())
			{
				switch (button)
				{
				case DialogMessageBoxButton.Yes:
					OnAccept.Run();
					break;
				case DialogMessageBoxButton.No:
					OnCancel.Run();
					break;
				case DialogMessageBoxButton.Close:
					OnCancel.Run();
					break;
				}
			}
		});
	}
}
