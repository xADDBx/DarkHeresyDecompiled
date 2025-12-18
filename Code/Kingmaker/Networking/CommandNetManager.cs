using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Net;
using Kingmaker.GameCommands;
using Kingmaker.MemoryPack.Formatters;
using Kingmaker.Pathfinding;
using Kingmaker.QA.Overlays;
using Kingmaker.UnitLogic.Commands.Base;
using OwlPack.Runtime;
using OwlPack.Unity;

namespace Kingmaker.Networking;

public class CommandNetManager
{
	static CommandNetManager()
	{
		RegisterCustomFormatters();
	}

	private static void RegisterCustomFormatters()
	{
		ExternalTypeLibrary.Instance.RegisterType(typeof(ForcedPath), typeof(ForcedPathSerializer));
		ExternalTypeLibrary.Instance.RegisterTypeForAllDerived(typeof(SimpleBlueprint), typeof(BlueprintSerializer<>));
		ExternalTypeLibrary.Instance.RegisterTypeForAllDerived(typeof(BlueprintReferenceBase), typeof(BlueprintReferenceSerializer<>));
		UnityTypesSerializers.Register();
	}

	public void SendAllCommands(int tickIndex, List<GameCommand> gameCommands, List<UnitCommandParams> unitCommands, List<SynchronizedData> synchronizedData)
	{
		ByteArraySlice byteArraySlice = NetMessageSerializer.SerializeToSlice(new UnitCommandMessage(tickIndex, gameCommands, unitCommands, synchronizedData));
		NetworkingOverlay.AddSentBytes(byteArraySlice.Count);
		if (!PhotonManager.Instance.SendMessageToOthers(7, byteArraySlice.Buffer, byteArraySlice.Offset, byteArraySlice.Count))
		{
			PFLog.Net.Error("Error when trying to send commands!");
		}
		byteArraySlice.Release();
	}

	public void OnCommandsReceived(NetPlayer player, ReadOnlySpan<byte> packet)
	{
		NetworkingOverlay.AddReceivedBytes(packet.Length);
		UnitCommandMessage unitCommandMessage;
		try
		{
			unitCommandMessage = NetMessageSerializer.DeserializeFromSpan<UnitCommandMessage>(packet);
			unitCommandMessage.AfterDeserialization();
		}
		catch (Exception ex)
		{
			PFLog.Net.Exception(ex, "Can't parse RecordElement!");
			return;
		}
		int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
		int tickIndex = unitCommandMessage.tickIndex;
		if (currentNetworkTick - 9 + 1 > tickIndex || tickIndex > currentNetworkTick + 18)
		{
			PFLog.Net.Error($"UnitCommandLockStep.LoadUnitCommandsInProcess: unexpected lockStepIndex! Current={currentNetworkTick} from packet={tickIndex} max={9}");
			return;
		}
		Game.Instance.GameCommandQueue.PushCommandsForPlayer(player, unitCommandMessage.tickIndex, unitCommandMessage.gameCommandList);
		Game.Instance.Controllers.UnitCommandBuffer.PushCommandsForPlayer(player, unitCommandMessage.tickIndex, unitCommandMessage.unitCommandList);
		Game.Instance.Controllers.SynchronizedDataController.PushDataForPlayer(player, unitCommandMessage.tickIndex, unitCommandMessage.synchronizedDataList);
	}
}
