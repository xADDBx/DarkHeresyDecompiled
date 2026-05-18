using System.Linq;
using Kingmaker.Code.Framework.TextTools;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Framework.TextTools;
using Kingmaker.TextTools.Base;
using Kingmaker.TextTools.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.TextTools;

public class TextTemplateEngine : BaseTextTemplateEngine
{
	private static readonly TextTemplateEngine s_Instance = new TextTemplateEngine();

	public static TextTemplateEngine Instance => s_Instance;

	private TextTemplateEngine()
	{
		AddTemplates();
	}

	private void AddTemplates()
	{
		AddTemplate("mf", new MaleFemaleTemplate());
		AddTemplate("rt_mf", new RtMaleFemaleTemplate());
		AddTemplate("unit_mf", new ContextUnitMaleFemaleTemplate());
		AddTemplate("race", new RaceTemplate());
		AddTemplate("name", new NameTemplate());
		AddTemplate("rt_name", new RtNameTemplate());
		AddTemplate("vo_name", new VoiceOverNameTemplate());
		AddTemplate("date", new DateTemplate());
		AddTemplate("time", new TimeTempate());
		AddTemplate("custom_companion_cost", new CustomCompanionCostTemplate());
		AddTemplate("respec_cost", new RespecCostTemplate());
		AddTemplate("n", new NarratorStartTemplate());
		AddTemplate("/n", new NarratorEndTemplate());
		AddTemplate("s", new StyleStartTemplate());
		AddTemplate("/s", new StyleEndTemplate());
		AddTemplate("g", new TooltipStartTemplate(TooltipType.Glosary));
		AddTemplate("/g", new TooltipEndTemplate(TooltipType.Glosary));
		AddTemplate("d", new TooltipStartTemplate(TooltipType.Decisions));
		AddTemplate("/d", new TooltipEndTemplate(TooltipType.Decisions));
		AddTemplate("m", new TooltipStartTemplate(TooltipType.Mechanics));
		AddTemplate("/m", new TooltipEndTemplate(TooltipType.Mechanics));
		AddTemplate("i", new ItalicStartTemplate());
		AddTemplate("/i", new ItalicEndTemplate());
		AddTemplate("b", new BoldStartTemplate());
		AddTemplate("/b", new BoldEndTemplate());
		AddTemplate("target", new LogTemplateTarget());
		AddTemplate("formula", new LogTemplateFormula());
		AddTemplate("source", new LogTemplateSource());
		AddTemplate("text", new LogTemplateText());
		AddTemplate("second_text", new LogTemplateSecondText());
		AddTemplate("description", new LogTemplateDescription());
		AddTemplate("count", new LogTemplateCount());
		AddTemplate("count_form", new LogTemplateCountForm());
		AddTemplate("roll", new LogTemplateRoll());
		AddTemplate("d100", GameLogTemplates.D100);
		AddTemplate("hit.d100", GameLogTemplates.HitD100);
		AddTemplate("body_part.d100", GameLogTemplates.BodyPartD100);
		AddTemplate("defence.d100", GameLogTemplates.DefenceD100);
		AddTemplate("cover_hit.d100", GameLogTemplates.CoverHitD100);
		AddTemplate("hit.chance", GameLogTemplates.HitChance);
		AddTemplate("dodge.chance", GameLogTemplates.DodgeChance);
		AddTemplate("parry.chance", GameLogTemplates.ParryChance);
		AddTemplate("rf.chance", GameLogTemplates.RfChance);
		AddTemplate("cover_hit.chance", GameLogTemplates.CoverHitChance);
		AddTemplate("total_hit.chance", GameLogTemplates.TotalHitChance);
		AddTemplate("target_superiority_penalty", GameLogTemplates.TargetSuperiorityPenalty);
		AddTemplate("pre_mitigation_damage", GameLogTemplates.PreMitigationDamage);
		AddTemplate("result_damage", GameLogTemplates.ResultDamage);
		AddTemplate("damage.difficulty_modifier", GameLogTemplates.DifficultyModifier);
		AddTemplate("mod", new LogTemplateModifier());
		AddTemplate("dc", new LogTemplateDC());
		AddTemplate("chance_dc", new LogTemplateChanceDC());
		AddTemplate("rations", new UITemplateRations());
		AddTemplate("recipe", new UITemplateSimpleText());
		AddTemplate("attack_number", new LogTemplateAttackNumber());
		AddTemplate("attacks_count", new LogTemplateAttacksCount());
		AddTemplate("round", new LogTemplateRound());
		AddTemplate("previous_value", new PreviousValueTemplate());
		AddTemplate("current_value", new CurrentValueTemplate());
		AddTemplate("target_value", new LogTemplateTrivial<int>(() => GameLogContext.TargetValue));
		AddTemplate("portraits_path", new UITemplatePortraitsPath());
		AddTemplate("area_name", new UITemplateAreaName());
		AddTemplate("unit_stat", new UnitStatStartTemplate());
		AddTemplate("armour.dodge", new UITemplateArmourDodge());
		AddTemplate("armour.damage_reduce", new UITemplateArmourDamageReduce());
		AddTemplate("price", new StringValueTemplate(() => GameLogContext.Price.ToString()));
		AddTemplate("bind", new KeyBindingTemplate());
		AddTemplate("console_bind", new ConsoleBindingTemplate());
		AddTemplate("empty", new EmptyTemplate());
		AddTemplate("br", new LineBreakTemplate());
		AddTemplate("pc_console", new PcConsoleTemplate());
		AddTemplate("t", new TutorialDataTemplate());
		AddTemplate("ui", new GlossaryTemplate());
		AddTemplate("gd", new GlossaryTemplate());
		AddTemplate("veil", new VeilTemplate());
		AddTemplate("veil_delta_value", new VeilDeltaValue());
		AddTemplate("veil_value", new VeilValue());
		AddTemplate("morale", new MoraleTemplate());
		AddTemplate("morale_start_value", new MoraleStartValue());
		AddTemplate("morale_result_value", new MoraleResultValue());
		AddTemplate("morale_phase", new MoralePhase());
		AddTemplate("push", new PushTemplate());
		AddTemplate("overpenetration", new OverpenetrationTemplate());
		AddTemplate("critical_hit", new CriticalHitTemplate());
		AddTemplate("vital_damage", new VitalDamageTemplate());
		AddTemplate("damage.type", new DamageTypeTemplate());
		AddTemplate("scaling_formula", new ScalingFormulaTemplate());
		AddTemplate("uip", new UIPropertyTemplate());
		AddTemplate("uicp", new UICommonPropertyTemplate());
		AddTemplate("caseId", new CaseIdTemplate());
		AddTemplate("clueId", new ClueIdTemplate());
		AddTemplate("reportId", new ReportIdTemplate());
		AddTemplate("case_name", new CaseNameTemplate());
		AddTemplate("case_item_name", new CaseItemNameTemplate());
		AddTemplate("case_item_area", new CaseItemAreaTemplate());
		AddTemplate("case_item", new CaseItemTemplate());
		AddTemplate("case_answer", new CaseAnswerTemplate());
		AddTemplate("bullet", new BulletTemplate());
	}

	public override string[] GetTemplateTags()
	{
		return BaseTextTemplateEngine.TemplatesByTag.Keys.ToArray();
	}
}
