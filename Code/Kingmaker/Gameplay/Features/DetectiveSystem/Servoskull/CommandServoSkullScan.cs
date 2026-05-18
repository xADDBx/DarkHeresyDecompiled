using System;
using System.Threading;
using System.Threading.Tasks;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[Serializable]
[ComponentName("Command/CommandServoSkullScan")]
[TypeId("0eb016294abd6e34aacac5b976de1d9c")]
public sealed class CommandServoSkullScan : CommandBase
{
	private class Data
	{
		public Task? ScanTask;

		public CancellationTokenSource? Cts;

		public bool Finished;
	}

	[SerializeReference]
	public MapObjectEvaluator? Target;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		PartDetectiveServoSkull partDetectiveServoSkull = PartDetectiveServoSkull.Find();
		if (Target == null || !Target.TryGetValue(out var value))
		{
			return CommandResult.Fail("Cant find target");
		}
		if (partDetectiveServoSkull == null)
		{
			commandData.Finished = true;
			return CommandResult.Fail("No servoskull");
		}
		if (skipping)
		{
			TeleportToScanPosition(partDetectiveServoSkull, value);
			commandData.Finished = true;
			return CommandResult.Success;
		}
		commandData.Cts = new CancellationTokenSource();
		commandData.ScanTask = RunScanAsync(commandData, partDetectiveServoSkull, value, commandData.Cts.Token);
		return CommandResult.Success;
	}

	private static async Task RunScanAsync(Data data, PartDetectiveServoSkull servoskull, MapObjectEntity target, CancellationToken ct)
	{
		try
		{
			await servoskull.Scan(target, ct);
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
		}
		catch (Exception ex2)
		{
			PFLog.Default.Exception(ex2);
		}
		finally
		{
			data.Finished = true;
		}
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		PartDetectiveServoSkull partDetectiveServoSkull = PartDetectiveServoSkull.Find();
		if (Target == null || !Target.TryGetValue(out var value))
		{
			return CommandResult.Fail("Cant find target");
		}
		if (partDetectiveServoSkull == null)
		{
			return CommandResult.Fail("No servoskull");
		}
		TeleportToScanPosition(partDetectiveServoSkull, value);
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).Finished;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Cts?.Cancel();
		commandData.Finished = true;
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		Task scanTask = player.GetCommandData<Data>(this).ScanTask;
		if (scanTask != null && !scanTask.IsCompleted)
		{
			PartDetectiveServoSkull partDetectiveServoSkull = PartDetectiveServoSkull.Find();
			MapObjectEntity mapObjectEntity = Target?.GetValue();
			if (partDetectiveServoSkull != null && mapObjectEntity != null)
			{
				TeleportToScanPosition(partDetectiveServoSkull, mapObjectEntity);
			}
		}
		return CommandResult.Success;
	}

	public override IAbstractUnitEntity? GetControlledUnit()
	{
		return PartDetectiveServoSkull.Find()?.Owner;
	}

	public override string GetCaption()
	{
		return "Servo-skull <b>scan</b> " + ((Target != null) ? Target.GetCaptionShort() : "???");
	}

	public override string? GetWarning()
	{
		if (Target != null && Target.CanEvaluate())
		{
			return null;
		}
		return "No target";
	}

	private static void TeleportToScanPosition(PartDetectiveServoSkull servoskull, MapObjectEntity target)
	{
		Vector3 normalized = (servoskull.Owner.Position - target.Position).normalized;
		float scanToTargetDistance = ConfigRoot.Instance.DetectiveServoskull.ScanToTargetDistance;
		Vector3 position = target.Position + normalized * scanToTargetDistance;
		servoskull.Owner.Translocate(position, null);
	}
}
