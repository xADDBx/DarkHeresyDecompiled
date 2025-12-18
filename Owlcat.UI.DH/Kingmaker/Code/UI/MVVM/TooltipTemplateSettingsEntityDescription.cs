using System.Collections.Generic;
using System.Linq;
using Code.View.UI.MVVM.Tooltip;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Localization;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateSettingsEntityDescription : TooltipBaseTemplate
{
	private readonly IUISettingsEntityBase m_SettingsEntity;

	private readonly string m_OwnTitle;

	private readonly string m_OwnDescription;

	public TooltipTemplateSettingsEntityDescription(IUISettingsEntityBase entity, string groupTitle = null, string ownDescription = null)
	{
		m_SettingsEntity = entity;
		m_OwnTitle = groupTitle;
		m_OwnDescription = ownDescription;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		LocalizedString localizedString = m_SettingsEntity?.Description;
		string text = ((localizedString != null) ? ((string)localizedString) : string.Empty);
		if (string.IsNullOrWhiteSpace(text))
		{
			text = m_OwnTitle ?? string.Empty;
		}
		yield return new TooltipBrickTitle(text, TooltipTitleType.H4, TextAlignmentOptions.BottomLeft, TextAnchor.LowerLeft);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		string text = m_OwnDescription;
		if (string.IsNullOrEmpty(text))
		{
			LocalizedString localizedString = m_SettingsEntity?.TooltipDescription;
			text = ((localizedString != null) ? ((string)localizedString) : string.Empty);
		}
		list.Add(new TooltipBrickSettingsText(text));
		if (m_SettingsEntity == null)
		{
			return list;
		}
		List<BlueprintEncyclopediaPageReference> encyclopediaDescription = m_SettingsEntity.EncyclopediaDescription;
		if (!encyclopediaDescription.Any())
		{
			return list;
		}
		foreach (BlueprintEncyclopediaPage item in from page in encyclopediaDescription
			select page?.Get() into blPage
			where blPage != null
			select blPage)
		{
			list.Add(new TooltipBrickTitle(item.GetTitle()));
			list.Add(new TooltipBrickSettingsText(UIUtilityCreateEncyclopediaTooltipDescription.CreateSettingsTooltipDescription(item)));
		}
		return list;
	}
}
