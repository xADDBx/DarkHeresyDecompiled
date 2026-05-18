using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.EntitySystem.Interfaces.View;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[OwlPackable(OwlPackableMode.Generate)]
public class DetectiveTraceEntity : MapObjectEntity, IAreaActivationHandler, ISubscriber, ITurnBasedModeHandler, IHashable, IOwlPackable<DetectiveTraceEntity>
{
	[JsonProperty]
	[OwlPackInclude]
	private DetectiveTraceStatus m_Status;

	private EntityRef<DetectiveTraceRootEntity> m_Root;

	private List<EntityRef<DetectiveTraceEntity>> m_Parents = new List<EntityRef<DetectiveTraceEntity>>();

	private EntityRef<DetectiveTraceEntity>[] m_Continuations;

	private bool m_TrueEnd;

	private bool m_HideInteract;

	private bool m_HideInteractionIfFollowed;

	private bool m_HideInteractionIfFollowedToDeadEnd;

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DetectiveTraceEntity",
		OldNames = null,
		Fields = new FieldInfo[18]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("m_Initiative", typeof(Initiative)),
			new FieldInfo("m_OriginalBlueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("m_Blueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("MainFact", typeof(MechanicEntityFact)),
			new FieldInfo("WasHighlightedOnRevealAndNoticed", typeof(bool)),
			new FieldInfo("ViewSettings", typeof(MapObjectViewSettings)),
			new FieldInfo("IsNewInGame", typeof(bool)),
			new FieldInfo("m_Status", typeof(DetectiveTraceStatus))
		}
	};

	public Vector3 FirstStepPosition { get; private set; }

	public Vector3 LastStepPosition { get; private set; }

	public DetectiveTraceStatus Status => m_Status;

	public InteractionPartDetectiveTrace? Interaction => GetOptional<InteractionPartDetectiveTrace>();

	public string DebugName => base.View?.gameObject?.name ?? base.UniqueId;

	public new IDetectiveTraceView View => (IDetectiveTraceView)base.View;

	public new IDetectiveTraceConfig Config => (IDetectiveTraceConfig)base.Config;

	public override float2 FoWPosition
	{
		get
		{
			Vector3 position = Config.PointsData.Last().Position;
			return new float2(position.x, position.z);
		}
	}

	public string DebugDump()
	{
		return $"trace {DebugName} status={m_Status} interactionEnabled={Interaction?.Enabled}" + $" hideInteract={m_HideInteract} hideIfFollowed={m_HideInteractionIfFollowed}" + $" hideIfDeadEnd={m_HideInteractionIfFollowedToDeadEnd} trueEnd={m_TrueEnd}" + " continuationStatuses=[" + string.Join(",", m_Continuations.Select((EntityRef<DetectiveTraceEntity> c) => c.Entity?.Status.ToString() ?? "null")) + "] parentStatuses=[" + string.Join(",", m_Parents.Select<EntityRef<DetectiveTraceEntity>, string>((EntityRef<DetectiveTraceEntity> p) => p.Entity?.Status.ToString() ?? "null")) + "] root=" + ((m_Root.Entity != null) ? "yes" : "no");
	}

	public DetectiveTraceEntity(IDetectiveTraceConfig config)
		: base(config)
	{
	}

	public DetectiveTraceEntity(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override IEntityView? CreateViewForData()
	{
		return null;
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		if (Config != null)
		{
			FirstStepPosition = Config.PointsData[0].Position;
			IReadOnlyList<DetectiveTracePoint> pointsData = Config.PointsData;
			Vector3 position2 = (LastStepPosition = pointsData[pointsData.Count - 1].Position);
			base.Position = position2;
			m_TrueEnd = Config.TrueEnd;
			m_HideInteract = Config.HideInteract;
			m_HideInteractionIfFollowed = Config.HideInteractionIfFollowed;
			m_HideInteractionIfFollowedToDeadEnd = Config.HideInteractionIfFollowedToDeadEnd;
			if (Config.Found && m_Status == DetectiveTraceStatus.None)
			{
				Found(initial: true);
			}
			SetInteractionEnabled(m_Status == DetectiveTraceStatus.Found);
		}
	}

	void IAreaActivationHandler.OnAreaActivated()
	{
		foreach (DetectiveTraceEntity item in Config.Continuations.Dereference().NotNull())
		{
			item.m_Parents.Add(new EntityRef<DetectiveTraceEntity>(this));
		}
		m_Continuations = Config.Continuations.ToArray();
		View.OnStatusLoad(Game.Instance.Controllers.TurnController.InCombat);
		PFLog.UI.Log("trace {0} OnAreaActivated: status={1} interactionEnabled={2} hideInteract={3} hideIfFollowed={4} hideIfDeadEnd={5} trueEnd={6} configFound={7} continuations={8} parents={9}", DebugName, m_Status, Interaction?.Enabled, m_HideInteract, m_HideInteractionIfFollowed, m_HideInteractionIfFollowedToDeadEnd, m_TrueEnd, Config.Found, m_Continuations.Length, m_Parents.Count);
	}

	public void Found(bool initial = false)
	{
		if (m_Status < DetectiveTraceStatus.Found)
		{
			m_Status = DetectiveTraceStatus.Found;
			SetInteractionEnabled(enabled: true);
			PFLog.UI.Log("trace {0} -> Found (initial={1})", DebugName, initial);
			if (!initial)
			{
				View.OnStatusChanged(Game.Instance.Controllers.TurnController.InCombat);
			}
		}
	}

	public void SetRoot(DetectiveTraceRootEntity root)
	{
		m_Root = root;
	}

	public void Followed()
	{
		if (!MaybeFollowedToDeadEnd() && m_Status < DetectiveTraceStatus.Followed)
		{
			m_Status = DetectiveTraceStatus.Followed;
			if (m_HideInteractionIfFollowed)
			{
				SetInteractionEnabled(enabled: false);
			}
			PFLog.UI.Log("trace {0} -> Followed: hideIfFollowed={1} continuations={2}", DebugName, m_HideInteractionIfFollowed, m_Continuations.Length);
			View.OnStatusChanged(Game.Instance.Controllers.TurnController.InCombat);
			EntityRef<DetectiveTraceEntity>[] continuations = m_Continuations;
			for (int i = 0; i < continuations.Length; i++)
			{
				((DetectiveTraceEntity)continuations[i]).Found();
			}
		}
	}

	private bool MaybeFollowedToDeadEnd()
	{
		if (m_Status >= DetectiveTraceStatus.FollowedToDeadEnd)
		{
			return true;
		}
		bool flag = m_Continuations.Empty() || m_Continuations.AllItems(delegate(EntityRef<DetectiveTraceEntity> i)
		{
			DetectiveTraceEntity entity = i.Entity;
			return entity != null && entity.Status >= DetectiveTraceStatus.FollowedToDeadEnd;
		});
		PFLog.UI.Log("trace {0} MaybeFollowedToDeadEnd: result={1} trueEnd={2} continuationStatuses=[{3}]", DebugName, flag, m_TrueEnd, string.Join(",", m_Continuations.Select((EntityRef<DetectiveTraceEntity> c) => c.Entity?.Status.ToString() ?? "null")));
		if (!flag)
		{
			return false;
		}
		if (m_TrueEnd)
		{
			ForceFollowedToTrueEndAll();
			return true;
		}
		m_Status = DetectiveTraceStatus.FollowedToDeadEnd;
		if (m_HideInteractionIfFollowedToDeadEnd)
		{
			SetInteractionEnabled(enabled: false);
		}
		View.OnStatusChanged(Game.Instance.Controllers.TurnController.InCombat);
		foreach (EntityRef<DetectiveTraceEntity> parent in m_Parents)
		{
			((DetectiveTraceEntity)parent).MaybeFollowedToDeadEnd();
		}
		return true;
	}

	public void ForceFollowedToTrueEndAll(string source = "self")
	{
		PFLog.UI.Log("trace {0} ForceFollowedToTrueEndAll: source={1} prevStatus={2}", DebugName, source, m_Status);
		if (m_Status >= DetectiveTraceStatus.FollowedToTrueEnd)
		{
			return;
		}
		m_Status = DetectiveTraceStatus.FollowedToTrueEnd;
		if (m_HideInteractionIfFollowedToDeadEnd)
		{
			SetInteractionEnabled(enabled: false);
		}
		View.OnStatusChanged(Game.Instance.Controllers.TurnController.InCombat);
		EntityRef<DetectiveTraceEntity>[] continuations = m_Continuations;
		for (int i = 0; i < continuations.Length; i++)
		{
			((DetectiveTraceEntity)continuations[i]).ForceFollowedToTrueEndAll("parent:" + DebugName);
		}
		foreach (EntityRef<DetectiveTraceEntity> parent in m_Parents)
		{
			((DetectiveTraceEntity)parent).ForceFollowedToTrueEndAll("child:" + DebugName);
		}
		if (m_Root.Entity != null)
		{
			m_Root.Entity.ForceFollowedToTrueEndAll();
		}
	}

	private void SetInteractionEnabled(bool enabled)
	{
		InteractionPartDetectiveTrace interaction = Interaction;
		if (interaction != null)
		{
			bool flag = enabled && !m_HideInteract;
			PFLog.UI.Log("trace {0} SetInteractionEnabled: requested={1} hideInteract={2} effective={3} status={4}", DebugName, enabled, m_HideInteract, flag, m_Status);
			interaction.Enabled = flag;
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		View.OnStatusChanged(Game.Instance.Controllers.TurnController.InCombat);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Status);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DetectiveTraceEntity source = new DetectiveTraceEntity(default(OwlPackConstructorParameter));
		result = Unsafe.As<DetectiveTraceEntity, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<DetectiveTraceEntity>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		formatter.Field(10, "m_Initiative", ref m_Initiative, state);
		formatter.Field(11, "m_OriginalBlueprint", ref m_OriginalBlueprint, state);
		formatter.Field(12, "m_Blueprint", ref m_Blueprint, state);
		MechanicEntityFact value2 = base.MainFact;
		formatter.Field(13, "MainFact", ref value2, state);
		bool value3 = base.WasHighlightedOnRevealAndNoticed;
		formatter.UnmanagedField(14, "WasHighlightedOnRevealAndNoticed", ref value3, state);
		MapObjectViewSettings value4 = base.ViewSettings;
		formatter.Field(15, "ViewSettings", ref value4, state);
		bool value5 = base.IsNewInGame;
		formatter.UnmanagedField(16, "IsNewInGame", ref value5, state);
		formatter.EnumField(17, "m_Status", ref m_Status, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DetectiveTraceEntity>();
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
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				m_Initiative = formatter.ReadPackable<Initiative>(state);
				break;
			case 11:
				m_OriginalBlueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 12:
				m_Blueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 13:
				base.MainFact = formatter.ReadPackable<MechanicEntityFact>(state);
				break;
			case 14:
				base.WasHighlightedOnRevealAndNoticed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 15:
				base.ViewSettings = formatter.ReadPackable<MapObjectViewSettings>(state);
				break;
			case 16:
				base.IsNewInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 17:
				m_Status = formatter.ReadEnum<DetectiveTraceStatus>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
