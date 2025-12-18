using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.ContextActions;

[TypeId("892a1b239add4a50b1f09acdbf98a54e")]
public class ContextActionSkillCheck : ContextAction
{
	public SkillType Skill;

	[SkillCheckActualDifficulty]
	public SkillCheckDifficulty Difficulty;

	[ShowIf("IsDifficultyCustom")]
	public ContextValue CustomDifficulty;

	public ActionList Success;

	public ActionList Fail;

	[UsedImplicitly]
	private bool IsDifficultyCustom => Difficulty == SkillCheckDifficulty.Custom;

	public override string GetCaption()
	{
		return string.Format("Check {0} with difficulty {1}{2}", Skill, Difficulty, (Difficulty == SkillCheckDifficulty.Custom) ? $"{CustomDifficulty}" : "");
	}

	protected override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity != null)
		{
			MechanicEntity attacker = base.Caster ?? entity;
			int difficulty = ((Difficulty == SkillCheckDifficulty.Custom) ? CustomDifficulty.Calculate(base.Context) : Difficulty.GetDC());
			if (Rulebook.Trigger(new RulePerformSkillCheck(entity, Skill.ToStatType(), difficulty, attacker)).ResultIsSuccess)
			{
				Success.Run();
			}
			else
			{
				Fail.Run();
			}
		}
	}
}
