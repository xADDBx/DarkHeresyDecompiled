using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Framework.Interaction;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Visual.Critters;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartFollowUnit : EntityPart<AbstractUnitEntity>, IHashable, IOwlPackable<UnitPartFollowUnit>
{
	private bool m_Initialized;

	private EntityRef<BaseUnitEntity> m_LeaderRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartFollowUnit",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public bool AlwaysRun { get; private set; }

	public bool CanBeSlowerThanLeader { get; private set; }

	public bool FollowWhileCutscene { get; private set; }

	public bool FollowInCombat { get; private set; }

	public bool Skip { get; set; }

	public BaseUnitEntity Leader => m_LeaderRef;

	public bool IsBusy => base.Owner.HasActiveInteraction();

	public void Init(BaseUnitEntity leader, FollowerSettings settings)
	{
		Init(leader, settings.AlwaysRun, settings.CanBeSlowerThanLeader, settings.FollowWhileCutscene, settings.FollowInCombat);
	}

	public void Init(BaseUnitEntity leader, EtudeBracketFollowUnit settings)
	{
		Init(leader, settings.AlwaysRun, settings.CanBeSlowerThanLeader, settings.FollowWhileCutscene);
	}

	public void Init(BaseUnitEntity leader, MakeUnitFollowUnit settings)
	{
		Init(leader, settings.AlwaysRun, settings.CanBeSlowerThanLeader, settings.FollowWhileCutscene);
	}

	private void Init(BaseUnitEntity leader, bool alwaysRun, bool canBeSlowerThanLeader, bool followWhileCutscene, bool followInCombat = true)
	{
		m_LeaderRef = leader;
		AlwaysRun = alwaysRun;
		CanBeSlowerThanLeader = canBeSlowerThanLeader;
		FollowWhileCutscene = followWhileCutscene;
		FollowInCombat = followInCombat;
		if (Leader == null)
		{
			PFLog.Default.Error("UnitPartFollowUnit.Init: Leader is null");
		}
		else
		{
			OnAttachOrPostLoad();
		}
	}

	protected override void OnAttachOrPostLoad()
	{
		if (Leader != null && !m_Initialized)
		{
			base.Owner.Sleepless.Retain();
			Leader.GetOrCreate<UnitPartFollowedByUnits>().AddFollower(base.Owner);
			m_Initialized = true;
		}
	}

	protected override void OnDetach()
	{
		if (m_Initialized)
		{
			base.Owner.Sleepless.Release();
			Leader.GetOrCreate<UnitPartFollowedByUnits>().RemoveFollower(base.Owner);
			m_Initialized = false;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartFollowUnit source = new UnitPartFollowUnit();
		result = Unsafe.As<UnitPartFollowUnit, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartFollowUnit>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartFollowUnit>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
