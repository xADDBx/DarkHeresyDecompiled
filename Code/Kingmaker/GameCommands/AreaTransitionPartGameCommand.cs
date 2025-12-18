using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class AreaTransitionPartGameCommand : GameCommandWithSynchronized, IOwlPackable<AreaTransitionPartGameCommand>
{
	public class TransitionExecutorEntity : ContextData<TransitionExecutorEntity>
	{
		public EntityRef<BaseUnitEntity> EntityRef { get; private set; }

		public TransitionExecutorEntity Setup(EntityRef<BaseUnitEntity> entityRef)
		{
			EntityRef = entityRef;
			return this;
		}

		protected override void Reset()
		{
			EntityRef = null;
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	private readonly EntityPartRef<Entity, AreaTransitionPart> m_AreaTransitionPartRef;

	[JsonProperty]
	[OwlPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_ExecutorEntity;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AreaTransitionPartGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_AreaTransitionPartRef", typeof(EntityPartRef<Entity, AreaTransitionPart>)),
			new FieldInfo("m_ExecutorEntity", typeof(EntityRef<BaseUnitEntity>))
		}
	};

	[JsonConstructor]
	public AreaTransitionPartGameCommand()
	{
	}

	private AreaTransitionPartGameCommand(EntityPartRef<Entity, AreaTransitionPart> m_areaTransitionPartRef, EntityRef<BaseUnitEntity> m_executorEntity)
	{
		m_AreaTransitionPartRef = m_areaTransitionPartRef;
		m_ExecutorEntity = m_executorEntity;
	}

	public AreaTransitionPartGameCommand([NotNull] AreaTransitionPart areaTransitionPart, bool isPlayerCommand, BaseUnitEntity executorEntity)
		: this(areaTransitionPart, executorEntity)
	{
		m_IsSynchronized = isPlayerCommand;
	}

	protected override void ExecuteInternal()
	{
		AreaTransitionPart entityPart = m_AreaTransitionPartRef.EntityPart;
		if (entityPart == null)
		{
			return;
		}
		if (Game.Instance.Player.IsInCombat)
		{
			PFLog.GameCommands.Log("[AreaTransitionPartGameCommand] Canceled due to IsInCombat=true");
			return;
		}
		if (Game.Instance.CurrentModeType == GameModeType.Dialog)
		{
			PFLog.GameCommands.Log("[AreaTransitionPartGameCommand] Canceled due to CurrentMode=Dialog");
			return;
		}
		if (Game.Instance.CurrentModeType == GameModeType.Cutscene)
		{
			PFLog.GameCommands.Log("[AreaTransitionPartGameCommand] Canceled due to CurrentMode=Cutscene");
			return;
		}
		if (Game.Instance.IsPaused)
		{
			Game.Instance.IsPaused = false;
		}
		BaseUnitEntity user = Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity u) => u.IsDirectlyControllable).FirstOrDefault((BaseUnitEntity u) => !u.IsPet);
		if (!entityPart.CheckRestrictions(user))
		{
			return;
		}
		ConditionAction conditionAction = entityPart.Blueprint?.Actions.FirstOrDefault((ConditionAction ca) => ca.Condition?.Check() ?? true);
		if (conditionAction != null)
		{
			using (ContextData<TransitionExecutorEntity>.Request().Setup(m_ExecutorEntity))
			{
				conditionAction.Actions.Run();
				return;
			}
		}
		BlueprintArea currentArea = Game.Instance.CurrentlyLoadedArea;
		BlueprintAreaEnterPoint targetEnterPoint = entityPart.AreaEnterPoint;
		EventBus.RaiseEvent(delegate(IPartyLeaveAreaHandler h)
		{
			h.HandlePartyLeaveArea(currentArea, targetEnterPoint);
		});
		Game.Instance.LoadArea(targetEnterPoint, entityPart.Settings.AutoSaveMode);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AreaTransitionPartGameCommand source = new AreaTransitionPartGameCommand();
		result = Unsafe.As<AreaTransitionPartGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AreaTransitionPartGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityPartRef<Entity, AreaTransitionPart> value = m_AreaTransitionPartRef;
		formatter.Field(0, "m_AreaTransitionPartRef", ref value, state);
		EntityRef<BaseUnitEntity> value2 = m_ExecutorEntity;
		formatter.Field(1, "m_ExecutorEntity", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AreaTransitionPartGameCommand>();
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
				Unsafe.AsRef(in m_AreaTransitionPartRef) = formatter.ReadPackable<EntityPartRef<Entity, AreaTransitionPart>>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_ExecutorEntity) = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
