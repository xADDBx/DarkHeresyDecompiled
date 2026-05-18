using System;

namespace Kingmaker.Framework.ContextContract.Roles;

public static class ContextEntryPointRoles
{
	private static readonly ContextRoleTable[] _Tables = BuildTables();

	public static ContextRoleTable For(ContextEntryPointKind kind)
	{
		if ((int)kind < 0 || (int)kind >= _Tables.Length)
		{
			throw new ArgumentOutOfRangeException("kind", kind, null);
		}
		return _Tables[(uint)kind];
	}

	private static ContextRoleTable[] BuildTables()
	{
		ContextEntryPointKind[] obj = (ContextEntryPointKind[])Enum.GetValues(typeof(ContextEntryPointKind));
		int num = 0;
		ContextEntryPointKind[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			int num2 = (int)array[i];
			if (num2 + 1 > num)
			{
				num = num2 + 1;
			}
		}
		ContextRoleTable[] array2 = new ContextRoleTable[num];
		for (int j = 0; j < num; j++)
		{
			array2[j] = ContextRoleTable.Empty;
		}
		array2[1] = ContextRoleTable.Empty.With(ContextField.Caster, new ContextRoleHint("ability caster")).With(ContextField.Owner, new ContextRoleHint("ability caster (== Caster in this scope)")).With(ContextField.Ability, new ContextRoleHint("the ability being cast"))
			.With(ContextField.ClickedTarget, new ContextRoleHint("originally clicked target (unit or point)"))
			.With(ContextField.Target, new ContextRoleHint("originally clicked target (== ClickedTarget)"));
		array2[3] = ContextRoleTable.Empty.With(ContextField.Caster, new ContextRoleHint("ability caster")).With(ContextField.Owner, new ContextRoleHint("ability caster")).With(ContextField.Ability, new ContextRoleHint("the ability being delivered"))
			.With(ContextField.ClickedTarget, new ContextRoleHint("originally clicked target"))
			.With(ContextField.Target, new ContextRoleHint("delivery target"));
		array2[4] = ContextRoleTable.Empty.With(ContextField.Caster, new ContextRoleHint("ability caster")).With(ContextField.Owner, new ContextRoleHint("ability caster")).With(ContextField.Ability, new ContextRoleHint("the ability being cast"))
			.With(ContextField.ClickedTarget, new ContextRoleHint("originally clicked target"))
			.With(ContextField.Target, new ContextRoleHint("originally clicked target"))
			.With(ContextField.Pattern, new ContextRoleHint("the ability's spatial pattern"));
		array2[6] = ContextRoleTable.Empty.With(ContextField.Caster, new ContextRoleHint("ability caster")).With(ContextField.Owner, new ContextRoleHint("ability caster")).With(ContextField.Ability, new ContextRoleHint("the ability being applied"))
			.With(ContextField.ClickedTarget, new ContextRoleHint("originally clicked target"))
			.With(ContextField.Target, new ContextRoleHint("per-iteration target (== CurrentEntity)"))
			.With(ContextField.CurrentEntity, new ContextRoleHint("per-iteration target entity"));
		array2[7] = ContextRoleTable.Empty.With(ContextField.Caster, new ContextRoleHint("ability caster")).With(ContextField.Owner, new ContextRoleHint("ability caster")).With(ContextField.Ability, new ContextRoleHint("the halo ability"))
			.With(ContextField.ClickedTarget, new ContextRoleHint("originally clicked target"))
			.With(ContextField.Target, new ContextRoleHint("originally clicked target"))
			.With(ContextField.Pattern, new ContextRoleHint("the halo's spatial pattern"));
		array2[8] = ContextRoleTable.Empty.With(ContextField.Caster, new ContextRoleHint("ability caster")).With(ContextField.Owner, new ContextRoleHint("ability caster")).With(ContextField.Ability, new ContextRoleHint("the halo ability"))
			.With(ContextField.CurrentEntity, new ContextRoleHint("entity in halo iteration"))
			.With(ContextField.Target, new ContextRoleHint("entity in halo iteration (== CurrentEntity)", null, "Use ContextMainTarget for the originally clicked target."))
			.With(ContextField.ClickedTarget, new ContextRoleHint("originally clicked target"))
			.With(ContextField.Pattern, new ContextRoleHint("the halo's spatial pattern"));
		array2[9] = ContextRoleTable.Empty.With(ContextField.Caster, new ContextRoleHint("ability caster")).With(ContextField.Owner, new ContextRoleHint("ability caster")).With(ContextField.Ability, new ContextRoleHint("the ability collecting targets"))
			.With(ContextField.ClickedTarget, new ContextRoleHint("originally clicked target"))
			.With(ContextField.Target, new ContextRoleHint("originally clicked target"));
		ContextRoleTable contextRoleTable = (array2[12] = (array2[11] = ContextRoleTable.Empty.With(ContextField.Caster, new ContextRoleHint("buff applier", null, "the unit that applied this buff. Falls back to Owner if not tracked.")).With(ContextField.Owner, new ContextRoleHint("buff holder", null, "the unit carrying this buff")).With(ContextField.Fact, new ContextRoleHint("the buff itself"))
			.With(ContextField.Blueprint, new ContextRoleHint("the buff blueprint"))
			.With(ContextField.Ability, new ContextRoleHint("ability that created the buff (if any)"))
			.With(ContextField.ClickedTarget, new ContextRoleHint("buff's original click target", null, "Falls back to Owner if buff was self-applied."))
			.With(ContextField.Target, new ContextRoleHint("buff's original click target (== ClickedTarget)"))));
		array2[13] = contextRoleTable.With(ContextField.Rule, new ContextRoleHint("the rule that fired this handler")).With(ContextField.RuleInitiator, new ContextRoleHint("initiator field of the firing rule", null, "specifics depend on rule type — see RuleProvenance")).With(ContextField.RuleTarget, new ContextRoleHint("target field of the firing rule", null, "specifics depend on rule type — see RuleProvenance"));
		ContextRoleTable contextRoleTable2 = (array2[14] = ContextRoleTable.Empty.With(ContextField.Caster, new ContextRoleHint("feature granter (or Owner if no granter)", null, "features are typically granted by character creation, level-up, or item grant — no actual caster. Falls back to Owner via Prepare().")).With(ContextField.Owner, new ContextRoleHint("feature holder", null, "the unit carrying this feature")).With(ContextField.Fact, new ContextRoleHint("the feature itself"))
			.With(ContextField.Blueprint, new ContextRoleHint("the feature blueprint"))
			.With(ContextField.Ability, new ContextRoleHint("ability that granted the feature (rare)"))
			.With(ContextField.ClickedTarget, new ContextRoleHint("feature's original click target", null, "Falls back to Owner when feature was not granted via an ability cast."))
			.With(ContextField.Target, new ContextRoleHint("feature's original click target (== ClickedTarget)")));
		array2[15] = contextRoleTable2.With(ContextField.Rule, new ContextRoleHint("the rule that fired this handler")).With(ContextField.RuleInitiator, new ContextRoleHint("initiator field of the firing rule", null, "specifics depend on rule type — see RuleProvenance")).With(ContextField.RuleTarget, new ContextRoleHint("target field of the firing rule", null, "specifics depend on rule type — see RuleProvenance"));
		return array2;
	}
}
