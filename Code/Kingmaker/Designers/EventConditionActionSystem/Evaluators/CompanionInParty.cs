using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/CompanionInParty")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("7aafe88b061e08e44aa3e725e8d8ff00")]
[OwlPackable(OwlPackableMode.Generate)]
public class CompanionInParty : AbstractUnitEvaluator, IOwlPackable<CompanionInParty>
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintUnitReference m_Companion;

	[Tooltip("Зарекручен и находится в активной партии")]
	public bool IncludeActive = true;

	[Tooltip("Зарекручен, находится в партии, но не управляется игроком в данный момент (стоит на месте, где находился в момент детача, пропадает из панели партии)")]
	public bool IncludeDetached;

	[Tooltip("Зарекручен, но в данный момент не находится в активной партии")]
	public bool IncludeRemote;

	[Tooltip("Анрекручен, т.е. удален из ростера")]
	public bool IncludeExCompanions;

	[ShowIf("IncludeExCompanions")]
	[Tooltip("Анрекручен как InReserve")]
	public bool IncludeInReserveEx;

	[ShowIf("IncludeExCompanions")]
	[Tooltip("Анрекручен как Kicked")]
	public bool IncludeKickedEx;

	[ShowIf("IncludeExCompanions")]
	[Tooltip("Анрекручен как Dead")]
	public bool IncludeDeadEx;

	[Tooltip("Индекс юнита, подпадающего под условия")]
	public int Index;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CompanionInParty",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public BlueprintUnit Companion => m_Companion;

	private bool IsCompanion(BlueprintUnit unit)
	{
		if (!m_Companion.Is(unit))
		{
			if (unit.PrototypeLink != null)
			{
				return IsCompanion((BlueprintUnit)unit.PrototypeLink);
			}
			return false;
		}
		return true;
	}

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return Game.Instance.Player.AllCrossSceneUnits.Where(IsMatchingFilters).Skip(Index).FirstOrDefault();
	}

	private bool IsMatchingFilters(BaseUnitEntity unit)
	{
		if (!IsCompanion(unit.Blueprint))
		{
			return false;
		}
		UnitPartCompanion optional = unit.GetOptional<UnitPartCompanion>();
		CompanionState companionState = optional?.State ?? CompanionState.None;
		CompanionExState companionExState = optional?.ExState ?? CompanionExState.InReserve;
		bool flag = !IncludeInReserveEx && !IncludeKickedEx && !IncludeDeadEx;
		switch (companionState)
		{
		case CompanionState.InParty:
			if (!IncludeActive)
			{
				break;
			}
			goto IL_00cf;
		case CompanionState.Remote:
			if (!IncludeRemote)
			{
				break;
			}
			goto IL_00cf;
		case CompanionState.ExCompanion:
			if ((!IncludeExCompanions || companionExState != 0 || !IncludeInReserveEx) && (!IncludeExCompanions || companionExState != CompanionExState.Kicked || !IncludeKickedEx) && (!IncludeExCompanions || companionExState != CompanionExState.Dead || !IncludeDeadEx) && !(IncludeExCompanions && flag))
			{
				break;
			}
			goto IL_00cf;
		case CompanionState.InPartyDetached:
			{
				if (!IncludeDetached)
				{
					break;
				}
				goto IL_00cf;
			}
			IL_00cf:
			return true;
		}
		return false;
	}

	public override string GetCaption()
	{
		return string.Format("Companion ({0}){1}", m_Companion.Get(), (Index > 0) ? $" #{Index}" : "");
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CompanionInParty source = new CompanionInParty();
		result = Unsafe.As<CompanionInParty, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CompanionInParty>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CompanionInParty>();
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
