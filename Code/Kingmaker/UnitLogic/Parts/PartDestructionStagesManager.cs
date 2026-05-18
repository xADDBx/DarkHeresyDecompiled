using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartDestructionStagesManager : MechanicEntityPart, IHashable, IOwlPackable<PartDestructionStagesManager>
{
	public interface IOwner : IEntityPartOwner<PartDestructionStagesManager>, IEntityPartOwner
	{
		PartDestructionStagesManager DestructionStages { get; }
	}

	private IDestructionStagesManager[] m_ViewManagers = new IDestructionStagesManager[0];

	private DestructionStage m_CurrentStage;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartDestructionStagesManager",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public PartHealth Health => base.Owner.GetRequired<PartHealth>();

	public DestructionStage Stage => CalculateDestructionStage();

	protected override void OnViewDidAttach()
	{
		Update(onLoad: true);
	}

	public void Update()
	{
		Update(onLoad: false);
	}

	public void UpdateOnIsInGameTrue()
	{
		Update(onLoad: true);
	}

	private void Update(bool onLoad)
	{
		if (base.Owner.View == null)
		{
			return;
		}
		if (onLoad)
		{
			m_ViewManagers = base.Owner.View.GetComponentsInChildren<IDestructionStagesManager>();
		}
		if (!onLoad && m_CurrentStage == Stage)
		{
			return;
		}
		m_CurrentStage = Stage;
		foreach (IDestructionStagesManager item in base.Owner.GetAll<IDestructionStagesManager>())
		{
			try
			{
				item.ChangeStage(Stage, onLoad);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex, "Exception occured in " + item.name);
			}
		}
		IDestructionStagesManager[] viewManagers = m_ViewManagers;
		foreach (IDestructionStagesManager destructionStagesManager in viewManagers)
		{
			try
			{
				destructionStagesManager.ChangeStage(Stage, onLoad);
			}
			catch (Exception ex2)
			{
				PFLog.Default.Exception(ex2, "Exception occured in " + destructionStagesManager.name);
			}
		}
	}

	private DestructionStage CalculateDestructionStage()
	{
		_ = (int)base.Owner.Actor.GetStat(StatType.MaxHitPoints, null, default(StatContext), "CalculateDestructionStage");
		if (Health.HitPointsLeft <= 0)
		{
			return DestructionStage.Destroyed;
		}
		return DestructionStage.Whole;
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
		PartDestructionStagesManager source = new PartDestructionStagesManager();
		result = Unsafe.As<PartDestructionStagesManager, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartDestructionStagesManager>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartDestructionStagesManager>();
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
