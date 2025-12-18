using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartActivatableAbility : BaseUnitPart, IHashable, IOwlPackable<UnitPartActivatableAbility>
{
	private class ActivatedWithCommandData
	{
		public TimeSpan Time;

		public ActivatableAbility Ability;

		public HashSet<ActivatableAbility> Activated;

		public int GroupSize;

		public bool IsExpired
		{
			get
			{
				if ((Activated?.Count ?? 0) + 1 < GroupSize)
				{
					return Game.Instance.Controllers.TimeController.GameTime - Time >= 1.Rounds().Seconds;
				}
				return true;
			}
		}

		public bool TryActivate(ActivatableAbility ability)
		{
			if (!IsExpired && Ability.IsActivateWithSameCommand(ability))
			{
				if (ability != Ability)
				{
					Activated = Activated ?? new HashSet<ActivatableAbility>();
					Activated.Add(ability);
				}
				return true;
			}
			return false;
		}

		public bool CanActivate(ActivatableAbility ability)
		{
			if (!IsExpired)
			{
				return Ability.IsActivateWithSameCommand(ability);
			}
			return false;
		}
	}

	private int[] m_GroupsSizeIncreases = new int[EnumUtils.GetMaxValuePlusOne<ActivatableAbilityGroup>()];

	private readonly List<ActivatedWithCommandData> m_ActivatedWithCommand = new List<ActivatedWithCommandData>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartActivatableAbility",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void IncreaseGroupSize(ActivatableAbilityGroup group)
	{
		m_GroupsSizeIncreases[(int)group]++;
	}

	public void DecreaseGroupSize(ActivatableAbilityGroup group)
	{
		m_GroupsSizeIncreases[(int)group]--;
	}

	public int GetGroupSize(ActivatableAbilityGroup group)
	{
		return m_GroupsSizeIncreases[(int)group] + 1;
	}

	public void OnActivatedWithCommand(ActivatableAbility ability)
	{
		int groupSize = GetGroupSize(ability.Blueprint.Group);
		if (groupSize >= 2 && ability.Blueprint.Group == ActivatableAbilityGroup.Judgment)
		{
			ActivatedWithCommandData activatedWithCommandData = m_ActivatedWithCommand.FirstItem((ActivatedWithCommandData i) => i.Ability == ability);
			if (activatedWithCommandData == null)
			{
				activatedWithCommandData = new ActivatedWithCommandData
				{
					Ability = ability
				};
				m_ActivatedWithCommand.Add(activatedWithCommandData);
			}
			activatedWithCommandData.Time = Game.Instance.Controllers.TimeController.GameTime;
			activatedWithCommandData.Activated?.Clear();
			activatedWithCommandData.GroupSize = groupSize;
		}
	}

	public bool TryActivateWithoutCommand(ActivatableAbility ability)
	{
		foreach (ActivatedWithCommandData item in m_ActivatedWithCommand)
		{
			if (item.TryActivate(ability))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanActivateWithoutCommand(ActivatableAbility ability)
	{
		foreach (ActivatedWithCommandData item in m_ActivatedWithCommand)
		{
			if (item.CanActivate(ability))
			{
				return true;
			}
		}
		return false;
	}

	public void Update()
	{
		m_ActivatedWithCommand.RemoveAll((ActivatedWithCommandData d) => d.IsExpired);
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
		UnitPartActivatableAbility source = new UnitPartActivatableAbility();
		result = Unsafe.As<UnitPartActivatableAbility, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartActivatableAbility>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartActivatableAbility>();
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
