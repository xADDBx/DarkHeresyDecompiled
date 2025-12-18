using System;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Commands;

public class UnitEndTurn : UnitCommand<UnitEndTurnParams>
{
	public override bool IsMoveUnit => false;

	public UnitEndTurn([NotNull] UnitEndTurnParams @params)
		: base(@params)
	{
	}

	protected override void OnTick()
	{
		if (base.Params.Delay != default(TimeSpan) && base.TimeSinceStart.Seconds() >= base.Params.Delay)
		{
			Game.Instance.Controllers.TurnController.RequestEndTurn();
			ForceFinish(ResultType.Success);
		}
	}

	protected override ResultType OnAction()
	{
		if (base.Params.Delay != default(TimeSpan))
		{
			return ResultType.None;
		}
		Game.Instance.Controllers.TurnController.RequestEndTurn();
		return ResultType.Success;
	}
}
