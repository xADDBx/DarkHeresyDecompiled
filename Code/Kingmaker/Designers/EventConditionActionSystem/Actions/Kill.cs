using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Kill")]
[AllowMultipleComponents]
[TypeId("abb0dcfdb51f3594ab0d2b1d28ecc782")]
public class Kill : GameAction
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class SilentDeathUnitPart : BaseUnitPart, IHashable, IOwlPackable<SilentDeathUnitPart>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "SilentDeathUnitPart",
			OldNames = null,
			Fields = new FieldInfo[0]
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			SilentDeathUnitPart source = new SilentDeathUnitPart();
			result = Unsafe.As<SilentDeathUnitPart, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<SilentDeathUnitPart>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SilentDeathUnitPart>();
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

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	[SerializeReference]
	public MechanicEntityEvaluator Killer;

	[Tooltip("Works only if the Killer is set. If 0, body just falls on the ground, 1 is a standard impulse. For bigger impulse, try set it up a bit higher.")]
	public int ImpulseMultiplier = 1;

	public UnitDismemberType Dismember;

	[ShowIf("LimpsApartSelected")]
	[SerializeField]
	private DismembermentLimbsApartType m_DismemberingAnimation;

	public bool DisableBattleLog;

	private bool LimpsApartSelected => Dismember == UnitDismemberType.LimbsApart;

	[UsedImplicitly]
	private bool HasKiller => Killer;

	public override string GetDescription()
	{
		return $"Убивает цель {Target}";
	}

	protected override void RunAction()
	{
		AbstractUnitEntity value = Target.GetValue();
		if (DisableBattleLog)
		{
			value.GetOrCreate<SilentDeathUnitPart>();
		}
		GameHelper.KillUnit(value, Killer ? Killer.GetValue() : null, ImpulseMultiplier, Dismember, LimpsApartSelected ? new DismembermentLimbsApartType?(m_DismemberingAnimation) : null);
	}

	public override string GetCaption()
	{
		return $"Kill ({Target})";
	}
}
