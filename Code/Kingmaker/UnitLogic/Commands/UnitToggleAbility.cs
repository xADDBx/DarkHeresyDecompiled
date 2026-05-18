using JetBrains.Annotations;
using Kingmaker.Code.Framework;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitToggleAbility : UnitCommand<UnitToggleAbilityParams>
{
	public override bool IsMoveUnit => false;

	public UnitToggleAbility([NotNull] UnitToggleAbilityParams @params)
		: base(@params)
	{
	}

	protected override ResultType OnAction()
	{
		ToggleAbility toggleAbility = base.Executor.ToggleAbilities.Get(base.Params.Blueprint);
		if (toggleAbility == null)
		{
			PFLog.Ability.Error($"Unable to toggle ability {base.Params.Blueprint} on unit {base.Executor}");
			return ResultType.Fail;
		}
		toggleAbility.Enabled = !toggleAbility.Enabled;
		Metrics.ToggleAbility.Id(toggleAbility.Blueprint.AssetGuid).State(toggleAbility.Enabled).Send();
		return ResultType.Success;
	}
}
