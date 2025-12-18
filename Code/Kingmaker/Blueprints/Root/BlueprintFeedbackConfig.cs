using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[TypeId("45b2290f110f41c39d03f3260cd62ff6")]
public class BlueprintFeedbackConfig : BlueprintScriptableObject
{
	[Header("IntroductoryText")]
	public InductoryText IntroductoryTextDefault;

	public InductoryText IntroductoryTextPS;

	public InductoryText IntroductoryTextMS;

	public InductoryText IntroductoryTextXBOX;

	public InductoryText IntroductoryTextSwitch;

	[Header("External Links")]
	public string ExternalURLCollection;

	public FeedbackPopupItem[] FallbackItems;

	[Header("Icons")]
	public Sprite Survey;

	public Sprite Discord;

	public Sprite Twitter;

	public Sprite Facebook;

	public Sprite Website;

	public Sprite GetIconByPopupItemType(FeedbackPopupItemType type)
	{
		return type switch
		{
			FeedbackPopupItemType.Survey => Survey, 
			FeedbackPopupItemType.Discord => Discord, 
			FeedbackPopupItemType.Twitter => Twitter, 
			FeedbackPopupItemType.Facebook => Facebook, 
			FeedbackPopupItemType.Website => Website, 
			_ => null, 
		};
	}

	public bool TryGetFallbackValue(FeedbackPopupItemType itemType, out FeedbackPopupItem item)
	{
		item = FallbackItems.FirstOrDefault((FeedbackPopupItem i) => i.ItemType == itemType);
		return item != null;
	}
}
