using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FeedbackPopupItemVM : ViewModel
{
	public readonly string Label;

	public readonly Sprite Icon;

	private readonly FeedbackPopupItem m_Config;

	public FeedbackPopupItemVM(FeedbackPopupItem config)
	{
		m_Config = config;
		Label = UIStrings.Instance.FeedbackPopupTexts.GetTitleByPopupItemType(config.ItemType);
		Icon = UIConfig.Instance.FeedbackConfig.GetIconByPopupItemType(config.ItemType);
	}

	public void HandleClick()
	{
		Application.OpenURL(m_Config.Url);
	}
}
