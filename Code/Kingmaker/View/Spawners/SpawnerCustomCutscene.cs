using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawnerBase))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("d6f2645cc07c4b578a5163423b9ffee3")]
public class SpawnerCustomCutscene : EntityPartComponent<SpawnerCustomCutscene.Part>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class Part : EntityPartWithConfig, IUnitInitializer, IAreaActivationHandler, ISubscriber, IHashable, IOwlPackable<Part>
	{
		private EntityRef<CutscenePlayerData> m_Cutscene;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "Part",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("SourceType", typeof(string))
			}
		};

		public new SpawnerCustomCutscene Source => (SpawnerCustomCutscene)base.Source;

		public void OnSpawn(AbstractUnitEntity unit)
		{
			if (m_Cutscene != null || Source.Cutscene?.Get() == null)
			{
				return;
			}
			using (ContextData<SpawnedUnitData>.Request().Setup(unit, base.ConcreteOwner.HoldingState))
			{
				CutscenePlayerView cutscenePlayerView = CutscenePlayerView.Play(Source.Cutscene.Get(), new ParametrizedContextSetter
				{
					AdditionalParams = { 
					{
						"Spawned",
						(INamedParameterValue)new NamedParameterValue_Unit(unit.FromAbstractUnitEntity())
					} }
				});
				m_Cutscene = cutscenePlayerView.PlayerData;
				cutscenePlayerView.PlayerData.TickScene();
			}
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
			if (m_Cutscene != null && m_Cutscene.Entity != null)
			{
				m_Cutscene.Entity.SetPaused(value: false, CutscenePauseReason.UnitSpawnerDoesNotControlAnyUnit);
			}
		}

		public void OnDispose(AbstractUnitEntity unit)
		{
			if (m_Cutscene != null && m_Cutscene.Entity != null)
			{
				m_Cutscene.Entity.SetPaused(value: true, CutscenePauseReason.UnitSpawnerDoesNotControlAnyUnit);
			}
		}

		public void OnAreaActivated()
		{
			if (Source.m_RestartIfAbsent)
			{
				AbstractUnitEntity spawnedUnit = ((AbstractUnitSpawnerEntity)base.ConcreteOwner).SpawnedUnit;
				if (spawnedUnit != null)
				{
					OnSpawn(spawnedUnit);
				}
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
			Part source = new Part();
			result = Unsafe.As<Part, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<Part>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			string value = base.SourceType;
			formatter.StringField(0, "SourceType", ref value, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Part>();
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

	[ValidateNotNull]
	public CutsceneReference Cutscene;

	[SerializeField]
	private bool m_RestartIfAbsent;
}
