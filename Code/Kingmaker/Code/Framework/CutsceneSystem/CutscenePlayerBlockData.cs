using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Framework.CutsceneSystem;

[OwlPackable(OwlPackableMode.Generate)]
public class CutscenePlayerBlockData : IHashable, IOwlPackable, IOwlPackable<CutscenePlayerBlockData>
{
	[OwlPackInclude]
	private string m_BlockGuid;

	[OwlPackInclude]
	private int m_AwaitingSignals;

	[OwlPackInclude]
	private bool m_IsComplete;

	[OwlPackInclude]
	private List<CutscenePlayerGateData> m_GatePlayers = new List<CutscenePlayerGateData>();

	[OwlPackInclude]
	private bool m_IsActivated;

	private CutsceneBlock m_Block;

	private CutscenePlayerData m_Player;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CutscenePlayerBlockData",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("m_BlockGuid", typeof(string)),
			new FieldInfo("m_AwaitingSignals", typeof(int)),
			new FieldInfo("m_IsComplete", typeof(bool)),
			new FieldInfo("m_GatePlayers", typeof(List<CutscenePlayerGateData>)),
			new FieldInfo("m_IsActivated", typeof(bool))
		}
	};

	public CutsceneBlock Block => m_Block;

	public bool IsComplete => m_IsComplete;

	public bool IsActivated => m_IsActivated;

	public bool CanBeActivated
	{
		get
		{
			if (!m_IsActivated && !m_IsComplete)
			{
				return m_AwaitingSignals == 0;
			}
			return false;
		}
	}

	public List<CutscenePlayerGateData> GatesPlayerData => m_GatePlayers;

	public CutscenePlayerBlockData(CutsceneBlock block, CutscenePlayerData player)
	{
		m_Block = block;
		m_Player = player;
		m_BlockGuid = block.Guid;
		int num = player.Cutscene.Blocks.Count((CutsceneBlock b) => b.NextBlocks.Contains(block.Guid)) + player.Cutscene.StageSwitches.Count((CutsceneStageSwitch s) => s.NextBlocks.Contains(block.Guid));
		switch (block.ActivationMode)
		{
		case CutsceneBlock.BlockActivationMode.AnySignal:
			m_AwaitingSignals = Math.Min(1, num);
			break;
		default:
			m_AwaitingSignals = num;
			break;
		}
		player.CutsceneSignals.RegisterBlock(this);
		CreateGatePlayers();
	}

	protected CutscenePlayerBlockData()
	{
	}

	private void CreateGatePlayers()
	{
		foreach (CutsceneGate gate in m_Block.Gates)
		{
			if (m_GatePlayers.Contains((CutscenePlayerGateData g) => g.Gate == gate))
			{
				return;
			}
			List<CutscenePlayerTrackData> tracksPlayerData = gate.Tracks.Select((CutsceneTrack t, int i) => new CutscenePlayerTrackData(t, i, m_Player)).ToList();
			CutscenePlayerGateData item = new CutscenePlayerGateData(gate, tracksPlayerData, m_Player);
			m_GatePlayers.Add(item);
		}
		foreach (CutscenePlayerGateData gatePlayer in m_GatePlayers)
		{
			for (int j = 0; j < gatePlayer.Tracks.Count; j++)
			{
				CutscenePlayerTrackData cutscenePlayerTrackData = gatePlayer.Tracks[j];
				foreach (string endGateId in cutscenePlayerTrackData.Track.EndGateIds)
				{
					CutsceneGate nextGate = Block.GetGateById(endGateId);
					if (nextGate != null)
					{
						m_GatePlayers.First((CutscenePlayerGateData g) => g.Gate == nextGate).AddIncomingTrack(cutscenePlayerTrackData);
					}
				}
			}
		}
	}

	public void SignalBlock()
	{
		m_AwaitingSignals--;
		if (m_AwaitingSignals == 0)
		{
			TickBlock(skipping: false);
		}
	}

	private void TryStartBlock()
	{
		if (m_AwaitingSignals > 0 || m_IsComplete || m_IsActivated)
		{
			return;
		}
		m_IsActivated = true;
		if (m_Block.IsDisabled)
		{
			CompleteBlock(shouldSignal: true);
			return;
		}
		foreach (CutscenePlayerGateData gatePlayer in m_GatePlayers)
		{
			gatePlayer.StartIfReady();
		}
	}

	private void CompleteBlock(bool shouldSignal)
	{
		m_IsComplete = true;
		foreach (CutscenePlayerGateData gatesPlayerDatum in GatesPlayerData)
		{
			if (gatesPlayerDatum.IsRunning)
			{
				gatesPlayerDatum.CheckCompletion();
			}
		}
		if (!shouldSignal)
		{
			return;
		}
		foreach (string nextBlock in m_Block.NextBlocks)
		{
			m_Player.CutsceneSignals.SignalBlock(nextBlock);
		}
	}

	public void TickBlock(bool skipping)
	{
		TryStartBlock();
		if (!m_IsActivated || m_IsComplete)
		{
			return;
		}
		foreach (CutscenePlayerGateData gatePlayer in m_GatePlayers)
		{
			gatePlayer.TickGate(skipping);
		}
		bool flag = false;
		bool flag2 = false;
		foreach (CutscenePlayerGateData gatePlayer2 in m_GatePlayers)
		{
			flag2 |= gatePlayer2.IsActivated && !gatePlayer2.IsNonContinuousPartCompleted;
			flag |= gatePlayer2.IsCancelled;
		}
		if (!flag2)
		{
			bool shouldSignal = !flag;
			CompleteBlock(shouldSignal);
		}
	}

	public void Stop()
	{
		foreach (CutscenePlayerGateData gatePlayer in m_GatePlayers)
		{
			if (gatePlayer.IsRunning)
			{
				gatePlayer.StopTracks();
			}
		}
	}

	public void Pause()
	{
		foreach (CutscenePlayerGateData gatePlayer in m_GatePlayers)
		{
			if (gatePlayer.IsRunning)
			{
				gatePlayer.PauseTracks();
			}
		}
	}

	public void Dispose()
	{
		Stop();
	}

	public void InterruptBark()
	{
		if (!m_Block.IsDisabled && !m_IsComplete && (TryInterruptCommandByType<CommandBarkUnit>() || TryInterruptCommandByType<CommandBarkEntity>()))
		{
			TryInterruptCommandByType<CommandUnitPlayCutsceneAnimation>();
		}
	}

	private bool TryInterruptCommandByType<T>()
	{
		bool flag = false;
		foreach (CutscenePlayerGateData gatePlayer in m_GatePlayers)
		{
			if (gatePlayer.IsRunning)
			{
				flag |= gatePlayer.InterruptCommand<T>();
			}
		}
		return flag;
	}

	public List<AbstractUnitEntity> GetCurrentControlledUnits()
	{
		List<AbstractUnitEntity> list = new List<AbstractUnitEntity>();
		foreach (CutscenePlayerGateData gatePlayer in m_GatePlayers)
		{
			if (gatePlayer.IsRunning)
			{
				List<AbstractUnitEntity> controlledUnits = gatePlayer.GetControlledUnits();
				if (controlledUnits != null)
				{
					list.AddRange(controlledUnits);
				}
			}
		}
		return list;
	}

	public void Resume()
	{
		if (m_IsComplete || !m_IsActivated)
		{
			return;
		}
		foreach (CutscenePlayerGateData gatePlayer in m_GatePlayers)
		{
			if (gatePlayer.IsRunning)
			{
				gatePlayer.Resume();
			}
		}
	}

	public bool TryRestoreOnPostLoad(BlueprintCutscene cutscene, CutscenePlayerData player)
	{
		CutsceneBlock cutsceneBlock = cutscene.Blocks.FirstOrDefault((CutsceneBlock b) => b.Guid == m_BlockGuid);
		if (cutsceneBlock == null)
		{
			return false;
		}
		m_Block = cutsceneBlock;
		m_Player = player;
		player.CutsceneSignals.RegisterBlock(this);
		bool flag = true;
		foreach (CutscenePlayerGateData gatesPlayerDatum in GatesPlayerData)
		{
			flag &= gatesPlayerDatum.TryRestoreOnPostLoad(cutsceneBlock, player);
		}
		return flag;
	}

	public void SignalGateExtra(string gateId)
	{
		CutscenePlayerGateData cutscenePlayerGateData = GatesPlayerData.FirstOrDefault((CutscenePlayerGateData p) => p.Gate.Guid == gateId);
		if (cutscenePlayerGateData != null)
		{
			AwaitExtraSignalsRecursively(cutscenePlayerGateData);
			cutscenePlayerGateData.ForceActivate();
			m_IsComplete = false;
		}
	}

	private void AwaitExtraSignalsRecursively(CutscenePlayerGateData gate)
	{
		HashSet<CutscenePlayerGateData> reactivateGates = new HashSet<CutscenePlayerGateData>();
		CollectGatesRecursively(gate);
		foreach (CutscenePlayerGateData item in reactivateGates)
		{
			foreach (CutscenePlayerTrackData track in item.Tracks)
			{
				foreach (string endGateId in track.Track.EndGateIds)
				{
					m_GatePlayers.FirstOrDefault((CutscenePlayerGateData g) => g.Gate.Guid == endGateId)?.AddIncomingTrack(track, canReactivate: true);
				}
			}
		}
		void CollectGatesRecursively(CutscenePlayerGateData gate)
		{
			reactivateGates.Add(gate);
			foreach (CutscenePlayerTrackData track2 in gate.Tracks)
			{
				foreach (string endGateId in track2.Track.EndGateIds)
				{
					CutscenePlayerGateData cutscenePlayerGateData = m_GatePlayers.FirstOrDefault((CutscenePlayerGateData g) => g.Gate.Guid == endGateId);
					reactivateGates.Add(cutscenePlayerGateData);
					CollectGatesRecursively(cutscenePlayerGateData);
				}
			}
		}
	}

	public void StopGateOnErrorInsideTrack(CutscenePlayerTrackData track)
	{
		foreach (CutscenePlayerGateData gatesPlayerDatum in GatesPlayerData)
		{
			foreach (CutscenePlayerTrackData track2 in gatesPlayerDatum.Tracks)
			{
				if (track2 == track)
				{
					gatesPlayerDatum.StopTracks(shouldSignal: true);
				}
			}
		}
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CutscenePlayerBlockData source = new CutscenePlayerBlockData();
		result = Unsafe.As<CutscenePlayerBlockData, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<CutscenePlayerBlockData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_BlockGuid", ref m_BlockGuid, state);
		formatter.UnmanagedField(1, "m_AwaitingSignals", ref m_AwaitingSignals, state);
		formatter.UnmanagedField(2, "m_IsComplete", ref m_IsComplete, state);
		formatter.Field(3, "m_GatePlayers", ref m_GatePlayers, state);
		formatter.UnmanagedField(4, "m_IsActivated", ref m_IsActivated, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CutscenePlayerBlockData>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				m_BlockGuid = formatter.ReadString(state);
				break;
			case 1:
				m_AwaitingSignals = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				m_IsComplete = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				m_GatePlayers = formatter.ReadPackable<List<CutscenePlayerGateData>>(state);
				break;
			case 4:
				m_IsActivated = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
