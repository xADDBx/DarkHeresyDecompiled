using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateGlossary : TooltipBaseTemplate
{
	public readonly BlueprintEncyclopediaGlossaryEntry GlossaryEntry;

	public readonly BlueprintEncyclopediaEntry EncyclopediaEntry;

	private readonly bool m_IsHistory;

	private readonly bool m_IsEncyclopedia;

	public TooltipTemplateGlossary(string key, bool isHistory = false, bool isEncyclopedia = false)
	{
		GlossaryEntry = UIUtilityEncyclopedy.GetGlossaryEntry(key);
		EncyclopediaEntry = UIUtilityEncyclopedy.GetEncyclopediaEntry(key);
		m_IsHistory = isHistory;
		m_IsEncyclopedia = isEncyclopedia;
	}

	public TooltipTemplateGlossary(BlueprintEncyclopediaGlossaryEntry glossaryEntry, bool isHistory = false, bool isEncyclopedia = false, BlueprintEncyclopediaEntry encyclopediaEntry = null)
	{
		GlossaryEntry = glossaryEntry;
		EncyclopediaEntry = encyclopediaEntry;
		m_IsHistory = isHistory;
		m_IsEncyclopedia = isEncyclopedia;
	}

	public TooltipTemplateGlossary(IEnumerable<string> keys, bool isHistory = false, bool isEncyclopedia = false)
	{
		foreach (string key in keys)
		{
			BlueprintEncyclopediaEntry encyclopediaEntry = UIUtilityEncyclopedy.GetEncyclopediaEntry(key);
			if (encyclopediaEntry != null)
			{
				EncyclopediaEntry = encyclopediaEntry;
			}
			BlueprintEncyclopediaGlossaryEntry glossaryEntry = UIUtilityEncyclopedy.GetGlossaryEntry(key);
			if (glossaryEntry != null)
			{
				GlossaryEntry = glossaryEntry;
				break;
			}
		}
		m_IsHistory = isHistory;
		m_IsEncyclopedia = isEncyclopedia;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (type == TooltipTemplateType.Info && m_IsHistory)
		{
			yield return new BrickHistoryManagementVM(GlossaryEntry, EncyclopediaEntry);
		}
		else
		{
			yield return new BrickTitleVM(EncyclopediaEntry?.Title ?? GlossaryEntry?.Title);
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		TooltipTextType flags = TooltipTextType.Paragraph | TooltipTextType.BlackColor;
		if ((type != TooltipTemplateType.Info || !m_IsHistory) && m_IsEncyclopedia)
		{
			flags |= TooltipTextType.GlossarySize;
		}
		string description = ((EncyclopediaEntry == null) ? GlossaryEntry?.GetDescription() : EncyclopediaEntry.GetTooltipInfo().FirstOrDefault()?.GetDescription());
		yield return new BrickSeparatorVM();
		yield return new BrickTextVM(description, flags);
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		RootUIContext instance = RootUIContext.Instance;
		FullScreenUIType fullScreenUIType = instance.FullScreenUIType;
		bool flag = fullScreenUIType == FullScreenUIType.Settings || fullScreenUIType == FullScreenUIType.NewGame || fullScreenUIType == FullScreenUIType.FirstLaunchSettings;
		bool flag2 = instance.FullScreenUIType == FullScreenUIType.Chargen;
		BlueprintEncyclopediaGlossaryEntry glossaryEntry = GlossaryEntry;
		if (glossaryEntry == null)
		{
			_ = EncyclopediaEntry?.HideInEncyclopedia;
		}
		else
		{
			_ = glossaryEntry.HideInEncyclopedia;
		}
		bool flag3 = true;
		if (type == TooltipTemplateType.Info && m_IsHistory && !flag && !flag2 && !flag3)
		{
			yield return new BrickButtonVM(EncyclopediaCallback, UIStrings.Instance.EncyclopediaTexts.EncyclopediaGlossaryButton);
		}
	}

	public override IEnumerable<ITooltipBrick> GetHint(TooltipTemplateType type)
	{
		if (m_IsEncyclopedia)
		{
			yield return new BrickHintVM(UIStrings.Instance.EncyclopediaTexts.TooltipOpenEncyclopedia);
		}
	}

	public void EncyclopediaCallback()
	{
		TutorialVM obj = RootVM.Instance?.TutorialVM.CurrentValue;
		obj?.BigWindowVM?.CurrentValue?.Hide();
		obj?.SmallWindowVM?.CurrentValue?.Hide();
		EventBus.RaiseEvent(delegate(IEncyclopediaGlossaryModeHandler h)
		{
			h.HandleGlossaryMode(state: false);
		});
		TooltipHelper.CloseGlossaryInfoWindow();
		TooltipHelper.HideInfo();
		UIUtilityEncyclopedy.ShowEncyclopediaPage(GlossaryEntry.Key);
	}
}
