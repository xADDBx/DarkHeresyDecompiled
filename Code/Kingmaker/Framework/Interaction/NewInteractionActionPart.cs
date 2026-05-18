using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Framework.Interaction;

[OwlPackable(OwlPackableMode.Generate)]
public class NewInteractionActionPart : NewInteractionPart<NewInteractionActionSettings>, IHashable, IOwlPackable<NewInteractionActionPart>
{
	private sealed class Process : InteractionProcess
	{
		private readonly BaseUnitEntity m_Initiator;

		private readonly NewInteractionActionPart m_Part;

		public Process(BaseUnitEntity initiator, AbstractUnitEntity processUser, MapObjectEntity target, NewInteractionActionPart part)
			: base(processUser, target)
		{
			m_Initiator = initiator;
			m_Part = part;
		}

		protected override Task RunProcess()
		{
			return RunProcessInternal();
		}

		private async Task RunProcessInternal()
		{
			foreach (InteractionModule module in m_Part.Settings.Modules)
			{
				if (module.CanInteract(base.Target))
				{
					await module.Execute(m_Initiator, base.Target);
				}
				else if (module.Required)
				{
					PFLog.Default.Error("Required module " + module.GetCaption() + " cannot interact but was not blocked");
				}
			}
			if (m_Part.Settings.DisableAfterUse)
			{
				m_Part.Enabled = false;
			}
			base.EventBus.RaiseEvent((IMapObjectEntity)base.Target, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
			{
				h.HandleObjectInteractChanged();
			}, isCheckRuntime: true);
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "NewInteractionActionPart",
		OldNames = null,
		Fields = new FieldInfo[6]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("AlreadyUnlocked", typeof(bool)),
			new FieldInfo("AlreadyVisited", typeof(bool)),
			new FieldInfo("m_LastCombatRoundInteractionAttempt", typeof(int)),
			new FieldInfo("m_Enabled", typeof(bool)),
			new FieldInfo("AlreadyUsed", typeof(bool))
		}
	};

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool AlreadyUsed { get; private set; }

	public override UIInteractionType UIInteractionType => base.Settings.UIType;

	public override InteractionType Type => base.Settings.InteractionType;

	public override bool NotInCombat => base.Settings.NotInCombat;

	public override int ApproachRadius => base.Settings.ProximityRadius;

	public override float OvertipVerticalCorrection => base.Settings.OvertipVerticalCorrection;

	public override bool CanInteract()
	{
		if (!base.CanInteract())
		{
			return false;
		}
		ConditionsHolder conditionsHolder = base.Settings.Conditions.Get();
		if ((bool)conditionsHolder && conditionsHolder.Conditions.HasConditions)
		{
			using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
			{
				if (!conditionsHolder.Conditions.Check())
				{
					return false;
				}
			}
		}
		foreach (InteractionModule module in base.Settings.Modules)
		{
			if (module.Required && !module.CanInteract(base.Owner))
			{
				return false;
			}
		}
		return true;
	}

	protected override bool CanBeSelected(BaseUnitEntity unit)
	{
		if (!base.CanBeSelected(unit))
		{
			return false;
		}
		foreach (InteractionModule module in base.Settings.Modules)
		{
			if (module.Required && !module.CanBeSelected(unit))
			{
				return false;
			}
		}
		return true;
	}

	protected override InteractionProcess OnInteract(BaseUnitEntity user)
	{
		AlreadyUsed = true;
		AbstractUnitEntity processUser = GetProcessUser(user, base.Settings.Modules, base.Owner);
		return new Process(user, processUser, base.Owner, this);
	}

	private static AbstractUnitEntity GetProcessUser(BaseUnitEntity initiator, List<InteractionModule> modules, MapObjectEntity owner)
	{
		foreach (InteractionModule module in modules)
		{
			if (module.Required || module.CanInteract(owner))
			{
				AbstractUnitEntity processUser = module.GetProcessUser(initiator);
				if (processUser != null)
				{
					return processUser;
				}
			}
		}
		return initiator;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = AlreadyUsed;
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		NewInteractionActionPart source = new NewInteractionActionPart();
		result = Unsafe.As<NewInteractionActionPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<NewInteractionActionPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = base.AlreadyUnlocked;
		formatter.UnmanagedField(1, "AlreadyUnlocked", ref value2, state);
		bool value3 = AlreadyVisited;
		formatter.UnmanagedField(2, "AlreadyVisited", ref value3, state);
		formatter.UnmanagedField(3, "m_LastCombatRoundInteractionAttempt", ref m_LastCombatRoundInteractionAttempt, state);
		formatter.UnmanagedField(4, "m_Enabled", ref m_Enabled, state);
		bool value4 = AlreadyUsed;
		formatter.UnmanagedField(5, "AlreadyUsed", ref value4, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<NewInteractionActionPart>();
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
			case 1:
				base.AlreadyUnlocked = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				AlreadyVisited = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				m_LastCombatRoundInteractionAttempt = formatter.ReadUnmanaged<int>(state);
				break;
			case 4:
				m_Enabled = formatter.ReadUnmanaged<bool>(state);
				break;
			case 5:
				AlreadyUsed = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
