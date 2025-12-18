using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIFeedbackPopupTexts
{
	public LocalizedString EndVersionMessage;

	public LocalizedString EndVersionButtonText;

	public LocalizedString Survey;

	public LocalizedString Discord;

	public LocalizedString Twitter;

	public LocalizedString Facebook;

	public LocalizedString Website;

	public string GetTitleByPopupItemType(FeedbackPopupItemType type)
	{
		return type switch
		{
			FeedbackPopupItemType.Survey => Survey, 
			FeedbackPopupItemType.Discord => Discord, 
			FeedbackPopupItemType.Twitter => Twitter, 
			FeedbackPopupItemType.Facebook => Facebook, 
			FeedbackPopupItemType.Website => Website, 
			_ => string.Empty, 
		};
	}
}
