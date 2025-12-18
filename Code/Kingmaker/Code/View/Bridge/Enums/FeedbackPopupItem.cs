using System;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Code.View.Bridge.Enums;

[Serializable]
[OwlPackOldName("Kingmaker.Code.UI.MVVM.FeedbackPopupItem, Code")]
public sealed class FeedbackPopupItem
{
	[JsonProperty]
	public FeedbackPopupItemType ItemType;

	[JsonProperty]
	public string Url;
}
