using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Localization;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateDetective : TooltipBaseTemplate
{
	private readonly SimpleBlueprint m_DetectiveBlueprint;

	private readonly bool m_UseHeader;

	private readonly string m_AdditionalDescription;

	public TooltipTemplateDetective(SimpleBlueprint blueprint, bool useHeader = true, string additionalDescription = null)
	{
		m_DetectiveBlueprint = blueprint;
		m_UseHeader = useHeader;
		m_AdditionalDescription = additionalDescription;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		SimpleBlueprint detectiveBlueprint;
		if (!m_UseHeader)
		{
			detectiveBlueprint = m_DetectiveBlueprint;
			if (!(detectiveBlueprint is BlueprintClue clue))
			{
				if (!(detectiveBlueprint is BlueprintCase @case))
				{
					if (!(detectiveBlueprint is BlueprintClueAddendum blueprintClueAddendum))
					{
						if (detectiveBlueprint is BlueprintConclusion)
						{
							yield return new BrickTitleVM(UIStrings.Instance.DetectiveDecor.ConclusionDesc.Text, TooltipTitleType.H1);
						}
					}
					else
					{
						yield return new BrickTitleVM(blueprintClueAddendum.Name.Text, TooltipTitleType.H1);
					}
				}
				else
				{
					yield return new BrickTitleVM(Game.Instance.DetectiveSystem.GetCaseDisplay(@case).Name, TooltipTitleType.H1);
				}
			}
			else
			{
				yield return new BrickTitleVM(clue.GetUIData().Name.Text, TooltipTitleType.H1);
			}
			yield break;
		}
		if (m_DetectiveBlueprint == null)
		{
			LocalizedString unknownCluesHeader = UIStrings.Instance.DetectiveJournal.UnknownCluesHeader;
			Sprite unknownCluesIcon = UIConfig.Instance.DetectiveConfig.UnknownCluesIcon;
			yield return new BrickEntityHeaderVM(unknownCluesHeader, unknownCluesIcon, hasUpgrade: false);
			yield break;
		}
		detectiveBlueprint = m_DetectiveBlueprint;
		if (!(detectiveBlueprint is BlueprintClue clue2))
		{
			if (!(detectiveBlueprint is BlueprintCase case2))
			{
				if (!(detectiveBlueprint is BlueprintClueAddendum blueprintClueAddendum2))
				{
					if (detectiveBlueprint is BlueprintConclusion)
					{
						yield return new BrickTitleVM(UIStrings.Instance.DetectiveDecor.ConclusionDesc.Text, TooltipTitleType.H1);
					}
				}
				else
				{
					yield return new BrickTitleVM(new TextEntity(blueprintClueAddendum2.Name.Text, TextFieldParams.Left), TooltipTitleType.H1);
				}
			}
			else
			{
				DetectiveSystem.CaseDisplayData caseDisplay = Game.Instance.DetectiveSystem.GetCaseDisplay(case2);
				yield return new BrickEntityHeaderVM(caseDisplay.Name, caseDisplay.Icon, hasUpgrade: false);
			}
		}
		else
		{
			yield return new BrickEntityHeaderVM(clue2.GetUIData().Name.Text, clue2.GetUIData().Icon, hasUpgrade: false);
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_DetectiveBlueprint == null)
		{
			list.Add(new BrickTextVM(UIStrings.Instance.DetectiveJournal.UnknownCluesDescription));
			return list;
		}
		SimpleBlueprint detectiveBlueprint = m_DetectiveBlueprint;
		string text = ((detectiveBlueprint is BlueprintClue clue) ? clue.GetUIData().Description.Text : ((detectiveBlueprint is BlueprintCase @case) ? Game.Instance.DetectiveSystem.GetCaseDisplay(@case).Description : ((detectiveBlueprint is BlueprintClueAddendum blueprintClueAddendum) ? blueprintClueAddendum.Description.Text : ((!(detectiveBlueprint is BlueprintConclusion blueprintConclusion)) ? string.Empty : blueprintConclusion.Description.Text))));
		string text2 = text;
		list.Add(new BrickTextVM(text2));
		if (m_AdditionalDescription == null)
		{
			return list;
		}
		IEnumerable<BrickTextVM> collection = new List<BrickTextVM>();
		detectiveBlueprint = m_DetectiveBlueprint;
		List<string> descs2;
		if (!(detectiveBlueprint is BlueprintClue clue2))
		{
			if (detectiveBlueprint is BlueprintCase parentCase && DetectiveInfoEncryption.TryDecryptConclusions(parentCase, m_AdditionalDescription, out var descs))
			{
				collection = descs.Select((string d) => new BrickTextVM(d));
				list.Add(new BrickTitleVM(UIStrings.Instance.Dialog.NewConclusionConstructed.Text, TooltipTitleType.H2));
			}
		}
		else if (DetectiveInfoEncryption.TryDecryptAddendums(clue2, m_AdditionalDescription, out descs2))
		{
			collection = descs2.Select((string d) => new BrickTextVM(d));
			list.Add(new BrickTitleVM(UIStrings.Instance.DetectiveJournal.NewAddendumLabel.Text, TooltipTitleType.H2));
		}
		list.AddRange(collection);
		return list;
	}
}
