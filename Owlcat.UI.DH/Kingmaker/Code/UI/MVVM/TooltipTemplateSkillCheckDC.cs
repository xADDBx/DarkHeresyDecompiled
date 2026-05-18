using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Dialog;
using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateSkillCheckDC : TooltipBaseTemplate
{
	private readonly List<SkillCheckDC> m_SkillCheckDcs;

	private SkillCheckDC m_Check;

	private string m_SkillName;

	private int m_ValueDC;

	private int m_SkillcheckChance;

	private int? m_FakeRoll;

	private int m_ConditionDC;

	private BaseUnitEntity m_ActingUnit;

	private UISkillcheckTooltip Texts => UIStrings.Instance.SkillcheckTooltips;

	public TooltipTemplateSkillCheckDC(List<SkillCheckDC> skillCheckDcs)
	{
		m_SkillCheckDcs = skillCheckDcs;
	}

	public override void Prepare(TooltipTemplateType type)
	{
		try
		{
			m_Check = m_SkillCheckDcs.FirstOrDefault();
			if (m_Check != null)
			{
				m_SkillName = UtilitySkillcheck.GetSkillCheckName(m_Check.StatType);
				m_ValueDC = m_Check.ValueDC;
				m_SkillcheckChance = UtilitySkillcheck.GetSkillCheckChance(m_Check);
				m_FakeRoll = m_Check.FakeRoll;
				m_ConditionDC = m_Check.ConditionDC;
				m_ActingUnit = m_Check.ActingUnit;
			}
		}
		catch (Exception ex)
		{
			PFLog.UI.Error(ex);
			m_Check = null;
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (m_Check != null)
		{
			yield return new BrickTitleVM(string.Format(UIStrings.Instance.Tooltips.TitlePreviewSkillcheckSkillDC, Texts.SkillCheck.Text, m_SkillName));
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (m_Check == null)
		{
			yield break;
		}
		if (m_ActingUnit != null)
		{
			foreach (ITooltipBrick item in FillForActingUnit())
			{
				yield return item;
			}
			yield break;
		}
		foreach (ITooltipBrick item2 in FillFreeSkillcheck())
		{
			yield return item2;
		}
	}

	private IEnumerable<ITooltipBrick> FillForActingUnit()
	{
		yield return new BrickPortraitAndNameVM(m_ActingUnit.Portrait.SmallPortrait, m_ActingUnit.CharacterName, new BrickTitleVM(new TextEntity($"{m_SkillName}: {m_ValueDC}", TextFieldParams.Left), TooltipTitleType.H6));
		yield return new BrickChanceVM(Texts.SkillCheckChance.Text, m_SkillcheckChance, m_FakeRoll, 0, isResultValue: true, null, CombatLogIcon.TargetHit);
		yield return new BrickTextValueVM(Texts.SkillValue.Text, m_ValueDC.ToString("+#;-#;0"), 1);
		if (m_ConditionDC != 0)
		{
			yield return new BrickTextValueVM(Texts.DifficultyClass.Text, m_ConditionDC.ToString("+#;-#;0"), 1);
		}
		if (m_Check.IsBestParameter && !m_Check.FakePassed.HasValue)
		{
			yield return new BrickSeparatorVM(TooltipBrickElementType.Small);
			yield return new BrickTextVM(UIStrings.Instance.Tooltips.TipPreviewSkillcheckBestCharacter, TooltipTextType.Italic | TooltipTextType.Centered, TooltipTextAlignment.Midl, m_ActingUnit);
		}
	}

	private IEnumerable<ITooltipBrick> FillFreeSkillcheck()
	{
		yield return new BrickTextVM(m_SkillName + ": " + UIUtilityText.AddSign(m_ValueDC) + "\n" + $"({UIStrings.Instance.SkillcheckTooltips.SkillCheckChance.Text}: {m_SkillcheckChance}%)", TooltipTextType.Centered);
		yield return new BrickIconValueStatVM(new TextValueElement(new TextEntity(Texts.DifficultyModRoll, TextFieldParams.Center), new TextEntity(m_ConditionDC.ToString())));
		yield return new BrickValueStatFormulaVM($"{m_ConditionDC.ToString()} {Texts.SkillValue.Text} + {m_ValueDC} {Texts.DifficultyClass.Text}", null, null);
	}
}
