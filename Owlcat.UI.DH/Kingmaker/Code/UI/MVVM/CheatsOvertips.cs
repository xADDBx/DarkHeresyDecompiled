using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using ObservableCollections;

namespace Kingmaker.Code.UI.MVVM;

internal static class CheatsOvertips
{
	[Cheat(Name = "dump_overtips", ExecutionPolicy = ExecutionPolicy.PlayMode, Description = "Print state of all live MapObject interaction overtip VMs.")]
	public static void DumpOvertips()
	{
		ObservableList<OvertipMapObjectVM> observableList = RootVM.Instance?.SurfaceOvertipsVM.Value?.MapObjectOvertipsVM?.MapInteractionObjectOvertipsCollectionVM?.Overtips;
		if (observableList == null)
		{
			PFLog.SmartConsole.Log("dump_overtips: no live overtips collection (SurfaceOvertipsVM not active)");
			return;
		}
		PFLog.SmartConsole.Log($"dump_overtips: {observableList.Count} overtip(s)");
		foreach (OvertipMapObjectVM item in observableList)
		{
			PFLog.SmartConsole.Log(FormatOvertipLine(item));
		}
	}

	[Cheat(Name = "dump_overtip", ExecutionPolicy = ExecutionPolicy.PlayMode, ExampleArgs = "TraceFromGardenTrue", Description = "Print full state for a MapObjectEntity by UniqueId or GameObject name.")]
	public static void DumpOvertip(string nameOrId)
	{
		if (string.IsNullOrEmpty(nameOrId))
		{
			PFLog.SmartConsole.Log("dump_overtip: usage — dump_overtip <UniqueId|gameObjectName>");
			return;
		}
		MapObjectEntity mapObjectEntity = FindMapObject(nameOrId);
		if (mapObjectEntity == null)
		{
			PFLog.SmartConsole.Log("dump_overtip: no MapObjectEntity matches '" + nameOrId + "'");
		}
		else
		{
			DumpEntity(mapObjectEntity);
		}
	}

	[Cheat(Name = "dump_traces", ExecutionPolicy = ExecutionPolicy.PlayMode, Description = "Print full state of every DetectiveTraceEntity and DetectiveTraceRootEntity in the current area.")]
	public static void DumpTraces()
	{
		List<DetectiveTraceRootEntity> list = Game.Instance.EntityPools.MapObjects.OfType<DetectiveTraceRootEntity>().ToList();
		List<DetectiveTraceEntity> list2 = Game.Instance.EntityPools.MapObjects.OfType<DetectiveTraceEntity>().ToList();
		PFLog.SmartConsole.Log($"dump_traces: {list.Count} root(s), {list2.Count} trace(s)");
		foreach (DetectiveTraceRootEntity item in list)
		{
			DumpEntity(item);
		}
		foreach (DetectiveTraceEntity item2 in list2)
		{
			DumpEntity(item2);
		}
	}

	private static MapObjectEntity FindMapObject(string nameOrId)
	{
		return Game.Instance.EntityPools.MapObjects.FirstOrDefault((MapObjectEntity m) => m.UniqueId == nameOrId || m.View?.gameObject?.name == nameOrId);
	}

	private static string FormatOvertipLine(OvertipMapObjectVM vm)
	{
		MapObjectEntity mapObjectEntity = vm.MapObjectEntity;
		string arg = mapObjectEntity?.View?.gameObject?.name ?? mapObjectEntity?.UniqueId ?? "(null)";
		AbstractInteractionPart firstInteractionPart = vm.FirstInteractionPart;
		string arg2 = ((mapObjectEntity is DetectiveTraceEntity detectiveTraceEntity) ? $" traceStatus={detectiveTraceEntity.Status}" : "");
		return $"  {arg}: IsEnabled={vm.IsEnabled.CurrentValue} NotAvailable={vm.NotAvailable}" + string.Format(" HasIntWithOvertip={0} part={1}", vm.HasInteractionsWithOvertip, firstInteractionPart?.GetType().Name ?? "null") + $" partEnabled={firstInteractionPart?.Enabled} canInteract={firstInteractionPart?.CanInteract()}{arg2}" + $" | visible={IsObjectVisible(vm)} hideFromScreen={vm.HideFromScreen}" + $" markedForRemoval={vm.IsMarkedForRemoval} inFrustum={vm.IsInCameraFrustum}";
	}

	private static bool IsObjectVisible(BaseOvertipMapObjectVM vm)
	{
		MapObjectEntity mapObjectEntity = vm.MapObjectEntity;
		if (mapObjectEntity != null && mapObjectEntity.IsVisibleForPlayer && !vm.IsMarkedForRemoval && !vm.HideFromScreen)
		{
			return vm.IsInCameraFrustum;
		}
		return false;
	}

	private static string FormatEntityVisibility(MapObjectEntity entity)
	{
		return $"isVisibleForPlayer={entity.IsVisibleForPlayer} isRevealed={entity.IsRevealed}" + $" isInState={entity.IsInState} isInGame={entity.IsInGame}" + $" isInFogOfWar={entity.IsInFogOfWar} isInCameraFrustum={entity.IsInCameraFrustum}";
	}

	private static void DumpEntity(MapObjectEntity entity)
	{
		string text = entity.View?.gameObject?.name ?? "(no view)";
		PFLog.SmartConsole.Log("--- " + entity.GetType().Name + " " + text + " (UniqueId=" + entity.UniqueId + ") ---");
		PFLog.SmartConsole.Log($"  position={entity.Position}");
		PFLog.SmartConsole.Log("  visibility: " + FormatEntityVisibility(entity));
		if (entity is DetectiveTraceEntity detectiveTraceEntity)
		{
			PFLog.SmartConsole.Log("  " + detectiveTraceEntity.DebugDump());
		}
		AbstractInteractionPart abstractInteractionPart = entity.Parts.GetAll<AbstractInteractionPart>().FirstOrDefault();
		if (abstractInteractionPart is NewInteractionPart newInteractionPart)
		{
			PFLog.SmartConsole.Log($"  interaction: type={newInteractionPart.GetType().Name} Enabled={newInteractionPart.Enabled} canInteract={newInteractionPart.CanInteract()}");
			if (newInteractionPart is InteractionPartDetectiveTrace interactionPartDetectiveTrace)
			{
				PartDetectiveServoSkull partDetectiveServoSkull = PartDetectiveServoSkull.Find();
				PFLog.SmartConsole.Log(string.Format("  detectiveTracePart: AlreadyUsed={0} IsVariative={1} servoskullBusy={2}", interactionPartDetectiveTrace.AlreadyUsed, interactionPartDetectiveTrace.IsVariative, (partDetectiveServoSkull != null) ? partDetectiveServoSkull.IsBusy.ToString() : "no-servo"));
			}
		}
		else if (abstractInteractionPart != null)
		{
			PFLog.SmartConsole.Log("  interaction: type=" + abstractInteractionPart.GetType().Name + " (not NewInteractionPart)");
		}
		else
		{
			PFLog.SmartConsole.Log("  interaction: none");
		}
		OvertipMapObjectVM overtipMapObjectVM = RootVM.Instance?.SurfaceOvertipsVM.Value?.MapObjectOvertipsVM?.MapInteractionObjectOvertipsCollectionVM?.Overtips?.FirstOrDefault((OvertipMapObjectVM v) => v.MapObjectEntity == entity);
		if (overtipMapObjectVM != null)
		{
			PFLog.SmartConsole.Log($"  overtipVM: IsEnabled={overtipMapObjectVM.IsEnabled.CurrentValue} NotAvailable={overtipMapObjectVM.NotAvailable} HasIntWithOvertip={overtipMapObjectVM.HasInteractionsWithOvertip}");
			PFLog.SmartConsole.Log($"  overtipVM visibility: visible={IsObjectVisible(overtipMapObjectVM)} hideFromScreen={overtipMapObjectVM.HideFromScreen} markedForRemoval={overtipMapObjectVM.IsMarkedForRemoval}");
		}
		else
		{
			PFLog.SmartConsole.Log("  overtipVM: NOT present in MapInteractionObjectOvertipsCollectionVM");
		}
	}
}
