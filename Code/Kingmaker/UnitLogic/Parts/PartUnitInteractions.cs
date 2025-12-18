using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Interaction;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
[OwlPackOldName("Kingmaker.UnitLogic.Parts.UnitPartInteractions")]
public class PartUnitInteractions : EntityPart<AbstractUnitEntity>, IHashable, IOwlPackable<PartUnitInteractions>
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("PartUnitInteractions");

	[NotNull]
	private readonly List<IUnitInteraction> m_Interactions = new List<IUnitInteraction>();

	public readonly Dictionary<BaseUnitEntity, float> Distances = new Dictionary<BaseUnitEntity, float>();

	public readonly List<float> Cooldowns = new List<float>();

	private bool m_HadMultipleClickInteractionsError;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartUnitInteractions",
		OldNames = new string[1] { "Kingmaker.UnitLogic.Parts.UnitPartInteractions" },
		Fields = new FieldInfo[0]
	};

	[NotNull]
	public IReadOnlyList<IUnitInteraction> Interactions => m_Interactions;

	public bool HasApproachInteractions { get; private set; }

	public bool HasDialogInteractions { get; private set; }

	public void AddInteraction(IUnitInteraction interaction)
	{
		m_Interactions.Insert(0, interaction);
		Cooldowns.Insert(0, 0f);
		CheckForApproachAndDialogInteractions();
	}

	private void CheckForApproachAndDialogInteractions()
	{
		HasApproachInteractions = false;
		HasDialogInteractions = false;
		foreach (IUnitInteraction interaction in m_Interactions)
		{
			HasApproachInteractions |= interaction.IsApproach;
			HasDialogInteractions |= interaction.IsDialog;
		}
	}

	public void UpdateCooldowns()
	{
		for (int i = 0; i < Cooldowns.Count; i++)
		{
			Cooldowns[i] = Math.Max(0f, Cooldowns[i] - Game.Instance.Controllers.TimeController.DeltaTime);
		}
	}

	[CanBeNull]
	public IUnitInteraction SelectClickInteraction(BaseUnitEntity initiator)
	{
		List<IUnitInteraction> list = TempList.Get<IUnitInteraction>();
		foreach (IUnitInteraction interaction in m_Interactions)
		{
			if (!interaction.IsApproach && interaction.IsAvailable(initiator, base.Owner))
			{
				list.Add(interaction);
			}
		}
		if (list.Count > 1 && !m_HadMultipleClickInteractionsError)
		{
			Logger.Error("Multiple click interactions with object {0}, selecting first available", base.Owner);
			m_HadMultipleClickInteractionsError = true;
		}
		return list.FirstOrDefault();
	}

	public void RemoveInteraction(IUnitInteraction interaction)
	{
		int num = m_Interactions.IndexOf(interaction);
		if (num != -1)
		{
			m_Interactions.RemoveAt(num);
			Cooldowns.RemoveAt(num);
		}
	}

	public void RemoveInteractions(Predicate<IUnitInteraction> pred)
	{
		int i;
		for (i = 0; i < m_Interactions.Count && !pred(m_Interactions[i]); i++)
		{
		}
		for (int j = i + 1; j < m_Interactions.Count; j++)
		{
			IUnitInteraction unitInteraction = m_Interactions[j];
			if (!pred(unitInteraction))
			{
				Cooldowns[i] = Cooldowns[j];
				m_Interactions[i] = unitInteraction;
				i++;
			}
		}
		m_Interactions.RemoveRange(i, m_Interactions.Count - i);
		Cooldowns.RemoveRange(i, m_Interactions.Count - i);
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
		PartUnitInteractions source = new PartUnitInteractions();
		result = Unsafe.As<PartUnitInteractions, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartUnitInteractions>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartUnitInteractions>();
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
