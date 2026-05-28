using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;

namespace Kingmaker.AreaLogic.Cutscenes;

[OwlPackable(OwlPackableMode.Generate)]
public class CutscenePlayerGateData : IOwlPackable, IOwlPackable<CutscenePlayerGateData>
{
	[OwlPackInclude]
	private string m_GateGuid;

	[OwlPackInclude]
	private List<CutscenePlayerTrackData> m_Tracks;

	[OwlPackInclude]
	private List<CutscenePlayerTrackData> m_IncomingTracks;

	[OwlPackInclude]
	private bool m_IsActivated;

	[OwlPackInclude]
	private bool m_IsCompleted;

	[OwlPackInclude]
	private bool m_IsStopped;

	[OwlPackInclude]
	private int m_AwaitingSignals;

	private CutsceneGate m_Gate;

	private CutscenePlayerData m_Player;

	private bool m_IsCancelled;

	private bool m_CanReactivate;

	private bool m_IsEndless;

	private bool m_IsNonContinuousPartCompleted;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CutscenePlayerGateData",
		OldNames = null,
		Fields = new FieldInfo[7]
		{
			new FieldInfo("m_GateGuid", typeof(string)),
			new FieldInfo("m_Tracks", typeof(List<CutscenePlayerTrackData>)),
			new FieldInfo("m_IncomingTracks", typeof(List<CutscenePlayerTrackData>)),
			new FieldInfo("m_IsActivated", typeof(bool)),
			new FieldInfo("m_IsCompleted", typeof(bool)),
			new FieldInfo("m_IsStopped", typeof(bool)),
			new FieldInfo("m_AwaitingSignals", typeof(int))
		}
	};

	public CutsceneGate Gate => m_Gate;

	public bool IsActivated => m_IsActivated;

	public bool IsNonContinuousPartCompleted => m_IsNonContinuousPartCompleted;

	public bool IsRunning
	{
		get
		{
			if (IsActivated)
			{
				return !IsCompleted;
			}
			return false;
		}
	}

	public bool IsCompleted => m_IsCompleted;

	public bool IsStopped => m_IsStopped;

	public bool IsCancelled => m_IsCancelled;

	public List<CutscenePlayerTrackData> Tracks => m_Tracks;

	public CutscenePlayerGateData(CutsceneGate gate, List<CutscenePlayerTrackData> tracksPlayerData, CutscenePlayerData player)
	{
		m_Gate = gate;
		m_GateGuid = gate.Guid;
		m_Player = player;
		m_Tracks = tracksPlayerData;
		m_IncomingTracks = new List<CutscenePlayerTrackData>();
		player.CutsceneSignals.RegisterGate(this);
	}

	protected CutscenePlayerGateData()
	{
	}

	public void AddIncomingTrack(CutscenePlayerTrackData track, bool canReactivate = false)
	{
		m_CanReactivate |= canReactivate;
		if (m_IncomingTracks.Contains(track))
		{
			return;
		}
		m_IncomingTracks.Add(track);
		if (Gate.TriggerType == CutsceneGate.GateTriggerType.AllTracks)
		{
			if (!track.Track.IsContinuous)
			{
				m_AwaitingSignals++;
			}
		}
		else
		{
			m_AwaitingSignals = 1;
		}
	}

	public void SignalGate()
	{
		m_AwaitingSignals--;
		if (m_AwaitingSignals <= 0)
		{
			if (m_IsActivated && m_CanReactivate)
			{
				Reset();
			}
			Activate();
		}
	}

	public void ForceActivate()
	{
		if (m_IsActivated)
		{
			Reset();
		}
		Activate();
	}

	private void Reset()
	{
		if (m_IsActivated && !m_IsCompleted)
		{
			foreach (CutscenePlayerTrackData track in m_Tracks)
			{
				track.ForceStop(shouldSignal: false, releaseControlledUnit: true);
			}
		}
		m_CanReactivate = false;
		m_IsActivated = false;
		m_IsCompleted = false;
		m_IsNonContinuousPartCompleted = false;
		m_IsStopped = false;
		m_IsCancelled = false;
		foreach (CutscenePlayerTrackData track2 in m_Tracks)
		{
			track2.Reset();
		}
	}

	public void StartIfReady()
	{
		if (m_AwaitingSignals <= 0 && !m_IsCompleted && !m_IsActivated)
		{
			Activate();
		}
	}

	private void Activate()
	{
		if (m_IsActivated || m_IsCompleted || m_IsStopped)
		{
			return;
		}
		foreach (CutscenePlayerTrackData incomingTrack in m_IncomingTracks)
		{
			if (incomingTrack.Track.IsContinuous)
			{
				incomingTrack.CompleteContinuous();
			}
		}
		m_IncomingTracks.Clear();
		m_IsActivated = true;
		if (m_Tracks == null || m_Tracks.Count == 0)
		{
			return;
		}
		m_IsEndless = Gate.Tracks.All((CutsceneTrack t) => t.IsContinuous && t.EndGateIds.Count == 0);
		switch (Gate.ActivationMode)
		{
		case CutsceneGate.ActivationModeType.AllTracks:
			m_Tracks.ForEach(delegate(CutscenePlayerTrackData t)
			{
				t.Start();
			});
			break;
		case CutsceneGate.ActivationModeType.FirstTrack:
			m_Tracks[0].Start();
			SkipNonActivated(0);
			break;
		case CutsceneGate.ActivationModeType.RandomTrack:
		{
			int num = Gate.Tracks.Sum((CutsceneTrack t) => t.RandomWeight);
			float num2 = m_Player.Random.Range(0f, num);
			int num3 = 0;
			for (int i = 0; i < m_Tracks.Count; i++)
			{
				num2 -= (float)m_Tracks[i].Track.RandomWeight;
				if (num2 <= 0f)
				{
					num3 = i;
					break;
				}
			}
			m_Tracks[num3].Start();
			SkipNonActivated(num3);
			break;
		}
		}
	}

	private void SkipNonActivated(int exclude)
	{
		bool shouldSignal = m_Gate.WhenTrackIsSkipped == CutsceneGate.SkipTracksModeType.SignalGate;
		for (int i = 0; i < m_Tracks.Count; i++)
		{
			if (i != exclude)
			{
				m_Tracks[i].JumpToEnd(shouldSignal);
			}
		}
	}

	public void TickGate(bool skipping)
	{
		if (!m_IsActivated || m_IsCompleted || m_IsStopped)
		{
			return;
		}
		bool flag = !m_IsEndless;
		foreach (CutscenePlayerTrackData track in m_Tracks)
		{
			track.Tick(skipping);
			if (!track.Track.IsContinuous)
			{
				flag &= track.IsCompleted;
			}
		}
		if (flag && !m_IsNonContinuousPartCompleted)
		{
			m_IsNonContinuousPartCompleted = true;
			m_IsCancelled = m_Tracks.All((CutscenePlayerTrackData t) => t.Canceled);
			if (Gate.Tracks.All((CutsceneTrack t) => !t.IsContinuous))
			{
				m_IsCompleted = true;
			}
		}
	}

	public void CheckCompletion()
	{
		if (m_Tracks.All((CutscenePlayerTrackData t) => t.IsCompleted))
		{
			m_IsCompleted = true;
		}
		else
		{
			PFLog.Cutscene.Warning($"Block completed while cutscene still has running tracks {m_Player.Cutscene}");
		}
	}

	public void StopTracks(bool shouldSignal = false)
	{
		m_IsStopped = true;
		m_Tracks.ForEach(delegate(CutscenePlayerTrackData t)
		{
			t.ForceStop(shouldSignal, releaseControlledUnit: true);
		});
	}

	public void PauseTracks()
	{
		m_IsStopped = true;
		m_Tracks.ForEach(delegate(CutscenePlayerTrackData t)
		{
			t.Pause();
		});
	}

	public List<AbstractUnitEntity> GetControlledUnits()
	{
		List<AbstractUnitEntity> list = new List<AbstractUnitEntity>();
		foreach (CutscenePlayerTrackData track in m_Tracks)
		{
			if (track.GetControlledUnits() != null)
			{
				list.AddRange(track.GetControlledUnits());
			}
		}
		return list;
	}

	public void Resume()
	{
		m_IsStopped = false;
		foreach (CutscenePlayerTrackData track in m_Tracks)
		{
			if (!track.IsCompleted)
			{
				track.Resume();
			}
		}
	}

	public bool InterruptCommand<T>()
	{
		bool flag = false;
		foreach (CutscenePlayerTrackData track in m_Tracks)
		{
			if (!track.IsCompleted)
			{
				flag |= track.InterruptCommand<T>();
			}
		}
		return flag;
	}

	public bool TryRestoreOnPostLoad(CutsceneBlock block, CutscenePlayerData player)
	{
		CutsceneGate cutsceneGate = block.Gates.FirstOrDefault((CutsceneGate g) => g.Guid == m_GateGuid);
		if (cutsceneGate == null)
		{
			return false;
		}
		m_Gate = cutsceneGate;
		m_Player = player;
		player.CutsceneSignals.RegisterGate(this);
		bool flag = true;
		foreach (CutscenePlayerTrackData track2 in m_Tracks)
		{
			CutsceneTrack track = (cutsceneGate.Tracks.IsValidIndex(track2.TrackIndex) ? cutsceneGate.Tracks[track2.TrackIndex] : null);
			flag &= track2.TryRestoreOnPostLoad(track, player);
		}
		m_IsEndless = Gate.Tracks.Count > 0 && Gate.Tracks.All((CutsceneTrack t) => t.IsContinuous && t.EndGateIds.Count == 0);
		return flag;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CutscenePlayerGateData source = new CutscenePlayerGateData();
		result = Unsafe.As<CutscenePlayerGateData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CutscenePlayerGateData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_GateGuid", ref m_GateGuid, state);
		formatter.Field(1, "m_Tracks", ref m_Tracks, state);
		formatter.Field(2, "m_IncomingTracks", ref m_IncomingTracks, state);
		formatter.UnmanagedField(3, "m_IsActivated", ref m_IsActivated, state);
		formatter.UnmanagedField(4, "m_IsCompleted", ref m_IsCompleted, state);
		formatter.UnmanagedField(5, "m_IsStopped", ref m_IsStopped, state);
		formatter.UnmanagedField(6, "m_AwaitingSignals", ref m_AwaitingSignals, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CutscenePlayerGateData>();
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
				m_GateGuid = formatter.ReadString(state);
				break;
			case 1:
				m_Tracks = formatter.ReadPackable<List<CutscenePlayerTrackData>>(state);
				break;
			case 2:
				m_IncomingTracks = formatter.ReadPackable<List<CutscenePlayerTrackData>>(state);
				break;
			case 3:
				m_IsActivated = formatter.ReadUnmanaged<bool>(state);
				break;
			case 4:
				m_IsCompleted = formatter.ReadUnmanaged<bool>(state);
				break;
			case 5:
				m_IsStopped = formatter.ReadUnmanaged<bool>(state);
				break;
			case 6:
				m_AwaitingSignals = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
