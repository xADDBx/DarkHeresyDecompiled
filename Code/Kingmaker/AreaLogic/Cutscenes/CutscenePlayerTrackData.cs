using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;

namespace Kingmaker.AreaLogic.Cutscenes;

[OwlPackable(OwlPackableMode.Generate)]
public class CutscenePlayerTrackData : IOwlPackable, IOwlPackable<CutscenePlayerTrackData>
{
	public class DebugTrackPlayerData
	{
		public bool SignalSent;

		public bool CantSkip;
	}

	[OwlPackInclude]
	private int m_TrackIndex;

	[OwlPackInclude]
	private bool m_IsActivated;

	[OwlPackInclude]
	private double m_PlayTime;

	[OwlPackInclude]
	private int m_RunningCommandIndex;

	[OwlPackInclude]
	private bool m_IsCompleted;

	[OwlPackInclude]
	private int m_TrackHash;

	[OwlPackInclude]
	private bool m_IsExtra;

	private CutsceneTrack m_Track;

	private List<CutscenePlayerCommandData> m_Commands;

	private CutscenePlayerData m_Player;

	private bool m_ForceStopped;

	private bool m_Canceled;

	public readonly DebugTrackPlayerData TrackDebugInfo = new DebugTrackPlayerData();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CutscenePlayerTrackData",
		OldNames = null,
		Fields = new FieldInfo[7]
		{
			new FieldInfo("m_TrackIndex", typeof(int)),
			new FieldInfo("m_IsActivated", typeof(bool)),
			new FieldInfo("m_PlayTime", typeof(double)),
			new FieldInfo("m_RunningCommandIndex", typeof(int)),
			new FieldInfo("m_IsCompleted", typeof(bool)),
			new FieldInfo("m_TrackHash", typeof(int)),
			new FieldInfo("m_IsExtra", typeof(bool))
		}
	};

	public CutsceneTrack Track => m_Track;

	public int TrackIndex => m_TrackIndex;

	public bool IsCompleted => m_IsCompleted;

	public bool Canceled => m_Canceled;

	public List<CutscenePlayerCommandData> CommandsPlayer => m_Commands;

	public CutscenePlayerTrackData(CutsceneTrack track, int trackIndex, CutscenePlayerData player)
	{
		m_Track = track;
		m_Player = player;
		m_TrackIndex = trackIndex;
		m_RunningCommandIndex = 0;
		m_Commands = new List<CutscenePlayerCommandData>();
		foreach (CommandReference command in m_Track.Commands)
		{
			if (command?.Get() != null && !command.Get().IsDisabled)
			{
				m_Commands.Add(new CutscenePlayerCommandData(command, this, player));
			}
		}
		m_TrackHash = track.GetHashMD5();
	}

	protected CutscenePlayerTrackData()
	{
	}

	public void Start()
	{
		if (IsCompleted)
		{
			return;
		}
		m_RunningCommandIndex = 0;
		m_PlayTime = 0.0;
		m_IsActivated = false;
		m_IsCompleted = false;
		foreach (CutscenePlayerCommandData command in m_Commands)
		{
			if (command.IsComplete)
			{
				command.Restart();
			}
			command.SetInitialDebugData();
		}
	}

	public void Tick(bool skipping)
	{
		if (IsCompleted)
		{
			return;
		}
		if (skipping && m_Track.Repeat && (!m_Track.Repeat || m_Player.Cutscene.NonSkippable))
		{
			skipping = false;
			TrackDebugInfo.CantSkip = true;
		}
		if (m_RunningCommandIndex >= m_Commands.Count && IsRepeat(skipping))
		{
			m_IsCompleted = false;
			Start();
		}
		int runningCommandIndex = m_RunningCommandIndex;
		for (; m_RunningCommandIndex < m_Commands.Count; m_RunningCommandIndex++)
		{
			CutscenePlayerCommandData cutscenePlayerCommandData = m_Commands[m_RunningCommandIndex];
			if (cutscenePlayerCommandData.IsDisabledDueToError)
			{
				continue;
			}
			if (m_RunningCommandIndex != runningCommandIndex || !m_IsActivated)
			{
				m_IsActivated = true;
				if (!CanRunCommand(cutscenePlayerCommandData))
				{
					break;
				}
				cutscenePlayerCommandData.Run(skipping);
				if (cutscenePlayerCommandData.IsComplete)
				{
					continue;
				}
			}
			cutscenePlayerCommandData.Tick(skipping);
			m_PlayTime = cutscenePlayerCommandData.PlayTime;
			if (!cutscenePlayerCommandData.IsComplete)
			{
				break;
			}
		}
		if (!IsCompleted && m_RunningCommandIndex >= m_Commands.Count && !IsRepeat(skipping))
		{
			CompleteTrack(shouldSignal: true);
		}
	}

	private bool IsRepeat(bool skipping)
	{
		if (m_Track.Repeat && !skipping)
		{
			return !m_ForceStopped;
		}
		return false;
	}

	private void CompleteTrack(bool shouldSignal)
	{
		m_IsCompleted = true;
		m_Player.FinishedTracks.Add(m_Track);
		if (!(!m_Track.IsContinuous && shouldSignal))
		{
			return;
		}
		foreach (string endGateId in m_Track.EndGateIds)
		{
			m_Player.CutsceneSignals.SignalGate(endGateId);
		}
		TrackDebugInfo.SignalSent = true;
	}

	private bool CanRunCommand(CutscenePlayerCommandData command)
	{
		var (flag, entryFailResult) = command.CheckEntryCondition();
		if (flag)
		{
			return true;
		}
		switch (entryFailResult)
		{
		case CommandBase.EntryFailResult.RemoveTrack:
			m_Canceled = true;
			JumpToEnd(shouldSignal: false);
			break;
		case CommandBase.EntryFailResult.FinishTrack:
			ForceStop(shouldSignal: true, releaseControlledUnit: true);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case CommandBase.EntryFailResult.SkipCommand:
			break;
		}
		return false;
	}

	public void ForceStop(bool shouldSignal, bool releaseControlledUnit)
	{
		if (!IsCompleted)
		{
			if (m_Commands.IsValidIndex(m_RunningCommandIndex))
			{
				m_Commands[m_RunningCommandIndex].ForceStop(releaseControlledUnit);
			}
			if (m_Player != null && m_Commands != null)
			{
				m_ForceStopped = true;
				JumpToEnd(shouldSignal);
			}
		}
	}

	public void Reset()
	{
		if (!IsCompleted)
		{
			ForceStop(shouldSignal: false, releaseControlledUnit: true);
		}
		m_RunningCommandIndex = 0;
		m_PlayTime = 0.0;
		m_IsActivated = false;
		m_IsCompleted = false;
	}

	public void Pause()
	{
		if (!IsCompleted)
		{
			if (m_Commands.IsValidIndex(m_RunningCommandIndex))
			{
				m_Commands[m_RunningCommandIndex].ForceStop(releaseControlledUnit: false);
			}
			m_ForceStopped = true;
		}
	}

	public void CompleteContinuous()
	{
		if (!IsCompleted)
		{
			if (m_Commands.IsValidIndex(m_RunningCommandIndex))
			{
				m_Commands[m_RunningCommandIndex].CompleteContinuous();
			}
			JumpToEnd(shouldSignal: false);
		}
	}

	public void JumpToEnd(bool shouldSignal)
	{
		m_RunningCommandIndex = m_Commands.Count;
		CompleteTrack(shouldSignal);
	}

	public List<AbstractUnitEntity> GetControlledUnits()
	{
		if (!IsCompleted)
		{
			return null;
		}
		List<AbstractUnitEntity> list = new List<AbstractUnitEntity>();
		IAbstractUnitEntity controlledUnit = m_Commands[m_RunningCommandIndex].GetControlledUnit();
		if (controlledUnit != null)
		{
			list.Add(controlledUnit.ToAbstractUnitEntity());
		}
		return list;
	}

	public void Resume()
	{
		m_ForceStopped = false;
		m_Commands[m_RunningCommandIndex].Resume();
	}

	public bool InterruptCommand<T>()
	{
		return m_Commands[m_RunningCommandIndex].Interrupt<T>();
	}

	public bool TryRestoreOnPostLoad(CutsceneTrack track, CutscenePlayerData player)
	{
		m_Track = track;
		m_Player = player;
		if (m_TrackHash != m_Track.GetHashMD5())
		{
			return false;
		}
		m_Commands = new List<CutscenePlayerCommandData>();
		foreach (CommandReference command in m_Track.Commands)
		{
			if (command?.Get() != null)
			{
				m_Commands.Add(new CutscenePlayerCommandData(command, this, player));
			}
		}
		if (m_RunningCommandIndex >= m_Commands.Count)
		{
			return true;
		}
		CutscenePlayerCommandData cutscenePlayerCommandData = m_Commands[m_RunningCommandIndex];
		IAbstractUnitEntity controlledUnit = cutscenePlayerCommandData.GetControlledUnit();
		if (controlledUnit != null && !cutscenePlayerCommandData.MarkUnit())
		{
			CutscenePlayerData.Logger.Error($"Cannot restore cutscene {m_Player.Cutscene} as another cutscene " + $"({controlledUnit.ToAbstractUnitEntity().CutsceneControlledUnit?.GetCurrentlyActive()}) controls an object ({controlledUnit})");
			return false;
		}
		return true;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CutscenePlayerTrackData source = new CutscenePlayerTrackData();
		result = Unsafe.As<CutscenePlayerTrackData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CutscenePlayerTrackData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_TrackIndex", ref m_TrackIndex, state);
		formatter.UnmanagedField(1, "m_IsActivated", ref m_IsActivated, state);
		formatter.UnmanagedField(2, "m_PlayTime", ref m_PlayTime, state);
		formatter.UnmanagedField(3, "m_RunningCommandIndex", ref m_RunningCommandIndex, state);
		formatter.UnmanagedField(4, "m_IsCompleted", ref m_IsCompleted, state);
		formatter.UnmanagedField(5, "m_TrackHash", ref m_TrackHash, state);
		formatter.UnmanagedField(6, "m_IsExtra", ref m_IsExtra, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CutscenePlayerTrackData>();
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
				m_TrackIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				m_IsActivated = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_PlayTime = formatter.ReadUnmanaged<double>(state);
				break;
			case 3:
				m_RunningCommandIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 4:
				m_IsCompleted = formatter.ReadUnmanaged<bool>(state);
				break;
			case 5:
				m_TrackHash = formatter.ReadUnmanaged<int>(state);
				break;
			case 6:
				m_IsExtra = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
