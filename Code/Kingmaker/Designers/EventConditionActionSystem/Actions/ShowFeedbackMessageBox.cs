using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("f774341e970749e3b395bd3e9c56b640")]
public class ShowFeedbackMessageBox : GameAction
{
	public ActionList OnClose;

	public int WaitTime;

	public override string GetCaption()
	{
		return "Show message box";
	}

	protected override void RunAction()
	{
		UtilityMessageBox.ShowMessageBox(UIStrings.Instance.FeedbackPopupTexts.EndVersionMessage.Text, DialogMessageBoxType.Message, delegate
		{
			FollowTheLink(FeedbackPopupItemType.Survey);
			OnClose.Run();
			Game.Instance.ResetToMainMenu();
		}, OnLinkInvoke, UIStrings.Instance.FeedbackPopupTexts.EndVersionButtonText.Text, null, WaitTime);
	}

	private void OnLinkInvoke(TMP_LinkInfo linkInfo)
	{
		if (!Enum.TryParse<FeedbackPopupItemType>(linkInfo.GetLinkID(), out var result))
		{
			Element.LogError("Cannot parse link feedback type!");
		}
		else
		{
			FollowTheLink(result);
		}
	}

	private static void FollowTheLink(FeedbackPopupItemType itemType)
	{
		if (FeedbackPopupConfigLoader.Instance.ItemsMap.TryGetValue(itemType, out var value) || UIConfig.Instance.FeedbackConfig.TryGetFallbackValue(itemType, out value))
		{
			Application.OpenURL(value.Url);
		}
		else
		{
			Element.LogError($"Cannot get feedback item with type: {itemType}!");
		}
	}
}
