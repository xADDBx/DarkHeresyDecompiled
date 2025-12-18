using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Covers;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[OwlPackable(OwlPackableMode.Generate)]
public class MapObjectProvidesCoverPart : ViewBasedPart<MapObjectForcedCoverSettings>, IDestructibleEntityHandler, ISubscriber<IMapObjectEntity>, ISubscriber, IDynamicCoverProvider, IHashable, IOwlPackable<MapObjectProvidesCoverPart>
{
	[HideInInspector]
	private DestructionStage m_CurrentDestructionStage;

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MapObjectProvidesCoverPart",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("SourceType", typeof(string))
		}
	};

	public NodeList Nodes => ((MapObjectEntity)base.Owner).GetOccupiedNodes();

	public LosCalculations.CoverType CoverType => base.Settings.CoverType;

	protected override void OnAttachOrPostLoad()
	{
		base.OnAttachOrPostLoad();
		Game.Instance.Controllers.ForcedCoversController.RegisterCoverProvider(this);
		PartDestructionStagesManager optional = base.Owner.GetOptional<PartDestructionStagesManager>();
		if (optional != null)
		{
			m_CurrentDestructionStage = optional.Stage;
		}
	}

	protected override void OnDetach()
	{
		Game.Instance.Controllers.ForcedCoversController.UnregisterCoverProvider(this);
		base.OnDetach();
	}

	protected override void OnSettingsDidSet(bool isNewSettings)
	{
		base.OnSettingsDidSet(isNewSettings);
		base.Settings.StageToCoverTypeMap = base.Settings.DestructionStageToCovers.ToDictionary((MapObjectForcedCoverSettings.DestructionStageToCover x) => x.DestructionStage, (MapObjectForcedCoverSettings.DestructionStageToCover x) => x.CoverType);
		UpdateCoverType();
	}

	public void HandleDestructionStageChanged(DestructionStage stage)
	{
		if (EventInvokerExtensions.MapObjectEntity == base.Owner)
		{
			m_CurrentDestructionStage = stage;
			UpdateCoverType();
		}
	}

	private void UpdateCoverType()
	{
		base.Settings.CoverType = (base.Settings.StageToCoverTypeMap.TryGetValue(m_CurrentDestructionStage, out var value) ? value : LosCalculations.CoverType.Obstacle);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MapObjectProvidesCoverPart source = new MapObjectProvidesCoverPart();
		result = Unsafe.As<MapObjectProvidesCoverPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MapObjectProvidesCoverPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MapObjectProvidesCoverPart>();
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
				base.SourceType = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
