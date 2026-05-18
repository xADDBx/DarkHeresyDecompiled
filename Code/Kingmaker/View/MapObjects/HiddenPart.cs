using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[OwlPackable(OwlPackableMode.Generate)]
public class HiddenPart : InteractionPart<HiddenSettings>, IHashable, IOwlPackable<HiddenPart>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "HiddenPart",
		OldNames = null,
		Fields = new FieldInfo[7]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("AlreadyUnlocked", typeof(bool)),
			new FieldInfo("AlreadyVisited", typeof(bool)),
			new FieldInfo("m_LastCombatRoundInteractionAttempt", typeof(int)),
			new FieldInfo("m_Enabled", typeof(bool)),
			new FieldInfo("Checked", typeof(bool)),
			new FieldInfo("Opened", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool Checked { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool Opened { get; private set; }

	public StatType StatType => base.Settings.StatType;

	public int DC => base.Settings.DifficultyClass;

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		SetHidden(!Opened);
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		if (!Checked)
		{
			RulePerformSkillCheck rulePerformSkillCheck = GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(user, base.Settings.StatType, base.Settings.DifficultyClass)
			{
				Voice = RulePerformSkillCheck.VoicingType.Success
			});
			Checked = true;
			Opened = rulePerformSkillCheck.ResultIsSuccess;
			SetHidden(!Opened);
		}
	}

	private void SetHidden(bool hidden)
	{
		foreach (AbstractInteractionPart item in base.Owner.Parts.GetAll<AbstractInteractionPart>())
		{
			if (item != this)
			{
				item.Enabled = !hidden;
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = Checked;
		result.Append(ref val2);
		bool val3 = Opened;
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		HiddenPart source = new HiddenPart();
		result = Unsafe.As<HiddenPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<HiddenPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = base.AlreadyUnlocked;
		formatter.UnmanagedField(1, "AlreadyUnlocked", ref value2, state);
		bool value3 = AlreadyVisited;
		formatter.UnmanagedField(2, "AlreadyVisited", ref value3, state);
		formatter.UnmanagedField(3, "m_LastCombatRoundInteractionAttempt", ref m_LastCombatRoundInteractionAttempt, state);
		formatter.UnmanagedField(4, "m_Enabled", ref m_Enabled, state);
		bool value4 = Checked;
		formatter.UnmanagedField(5, "Checked", ref value4, state);
		bool value5 = Opened;
		formatter.UnmanagedField(6, "Opened", ref value5, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<HiddenPart>();
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
				Checked = formatter.ReadUnmanaged<bool>(state);
				break;
			case 6:
				Opened = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
