using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.DialogSystem.State;

[OwlPackable(OwlPackableMode.Generate)]
public class DialogState : Entity, IHashable, IOwlPackable<DialogState>
{
	public const string ID = "dialog-state-id";

	public new static readonly EntityRef<DialogState> Ref = new EntityRef<DialogState>("dialog-state-id");

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DialogState",
		OldNames = null,
		Fields = new FieldInfo[16]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("SelectedAnswers", typeof(HashSet<string>)),
			new FieldInfo("AnswerChecks", typeof(Dictionary<string, CheckResult>)),
			new FieldInfo("ShownAnswerLists", typeof(HashSet<string>)),
			new FieldInfo("ShownCues", typeof(HashSet<string>)),
			new FieldInfo("ShownDialogs", typeof(HashSet<string>)),
			new FieldInfo("BookEventLog", typeof(Dictionary<BlueprintDialog, List<BlueprintScriptableObject>>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public HashSet<string> SelectedAnswers { get; private set; } = new HashSet<string>();


	[JsonProperty]
	[OwlPackInclude]
	public Dictionary<string, CheckResult> AnswerChecks { get; private set; } = new Dictionary<string, CheckResult>();


	[JsonProperty]
	[OwlPackInclude]
	public HashSet<string> ShownAnswerLists { get; private set; } = new HashSet<string>();


	[JsonProperty]
	[OwlPackInclude]
	public HashSet<string> ShownCues { get; private set; } = new HashSet<string>();


	[JsonProperty]
	[OwlPackInclude]
	public HashSet<string> ShownDialogs { get; private set; } = new HashSet<string>();


	[JsonProperty]
	[HasherCustom(Type = typeof(DictionaryBlueprintToListOfBlueprintsHasher<BlueprintDialog, BlueprintScriptableObject>))]
	[OwlPackInclude]
	public Dictionary<BlueprintDialog, List<BlueprintScriptableObject>> BookEventLog { get; private set; } = new Dictionary<BlueprintDialog, List<BlueprintScriptableObject>>();


	public DialogState()
		: base("dialog-state-id", isInGame: true)
	{
	}

	public DialogState(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override IEntityView CreateViewForData()
	{
		return null;
	}

	public bool SelectedAnswersContains(BlueprintAnswer bp)
	{
		return SelectedAnswers.Contains(bp.AssetGuid);
	}

	public bool SelectedAnswersAdd(BlueprintAnswer bp)
	{
		return SelectedAnswers.Add(bp.AssetGuid);
	}

	public bool SelectedAnswersRemove(BlueprintAnswer bp)
	{
		return SelectedAnswers.Remove(bp.AssetGuid);
	}

	public bool AnswerChecksContains(BlueprintAnswer bp)
	{
		return AnswerChecks.ContainsKey(bp.AssetGuid);
	}

	public bool AnswerChecksTryGetValue(BlueprintAnswer bp, out CheckResult res)
	{
		return AnswerChecks.TryGetValue(bp.AssetGuid, out res);
	}

	public CheckResult AnswerChecksGet(BlueprintAnswer bp)
	{
		return AnswerChecks[bp.AssetGuid];
	}

	public void AnswerChecksAdd(BlueprintAnswer bp, CheckResult result)
	{
		AnswerChecks.Add(bp.AssetGuid, result);
	}

	public bool ShownAnswerListsContains(BlueprintAnswersList bp)
	{
		return ShownAnswerLists.Contains(bp.AssetGuid);
	}

	public bool ShownAnswerListsAdd(BlueprintAnswersList bp)
	{
		return ShownAnswerLists.Add(bp.AssetGuid);
	}

	public bool ShownCuesContains(BlueprintCueBase bp)
	{
		return ShownCues.Contains(bp.AssetGuid);
	}

	public bool ShownCuesAdd(BlueprintCueBase bp)
	{
		return ShownCues.Add(bp.AssetGuid);
	}

	public bool ShownCuesRemove(BlueprintCueBase bp)
	{
		return ShownCues.Remove(bp.AssetGuid);
	}

	public bool ShownDialogsContains(BlueprintDialog bp)
	{
		return ShownDialogs.Contains(bp.AssetGuid);
	}

	public bool ShownDialogsAdd(BlueprintDialog bp)
	{
		return ShownDialogs.Add(bp.AssetGuid);
	}

	public bool ShownDialogsRemove(BlueprintDialog bp)
	{
		return ShownDialogs.Remove(bp.AssetGuid);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		HashSet<string> selectedAnswers = SelectedAnswers;
		if (selectedAnswers != null)
		{
			int num = 0;
			foreach (string item in selectedAnswers)
			{
				num ^= StringHasher.GetHash128(item).GetHashCode();
			}
			result.Append(num);
		}
		Dictionary<string, CheckResult> answerChecks = AnswerChecks;
		if (answerChecks != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<string, CheckResult> item2 in answerChecks)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = StringHasher.GetHash128(item2.Key);
				hash.Append(ref val3);
				CheckResult obj = item2.Value;
				Hash128 val4 = UnmanagedHasher<CheckResult>.GetHash128(ref obj);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		HashSet<string> shownAnswerLists = ShownAnswerLists;
		if (shownAnswerLists != null)
		{
			int num2 = 0;
			foreach (string item3 in shownAnswerLists)
			{
				num2 ^= StringHasher.GetHash128(item3).GetHashCode();
			}
			result.Append(num2);
		}
		HashSet<string> shownCues = ShownCues;
		if (shownCues != null)
		{
			int num3 = 0;
			foreach (string item4 in shownCues)
			{
				num3 ^= StringHasher.GetHash128(item4).GetHashCode();
			}
			result.Append(num3);
		}
		HashSet<string> shownDialogs = ShownDialogs;
		if (shownDialogs != null)
		{
			int num4 = 0;
			foreach (string item5 in shownDialogs)
			{
				num4 ^= StringHasher.GetHash128(item5).GetHashCode();
			}
			result.Append(num4);
		}
		Hash128 val5 = DictionaryBlueprintToListOfBlueprintsHasher<BlueprintDialog, BlueprintScriptableObject>.GetHash128(BookEventLog);
		result.Append(ref val5);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DialogState source = new DialogState(default(OwlPackConstructorParameter));
		result = Unsafe.As<DialogState, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DialogState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		HashSet<string> value2 = SelectedAnswers;
		formatter.Field(10, "SelectedAnswers", ref value2, state);
		Dictionary<string, CheckResult> value3 = AnswerChecks;
		formatter.Field(11, "AnswerChecks", ref value3, state);
		HashSet<string> value4 = ShownAnswerLists;
		formatter.Field(12, "ShownAnswerLists", ref value4, state);
		HashSet<string> value5 = ShownCues;
		formatter.Field(13, "ShownCues", ref value5, state);
		HashSet<string> value6 = ShownDialogs;
		formatter.Field(14, "ShownDialogs", ref value6, state);
		Dictionary<BlueprintDialog, List<BlueprintScriptableObject>> value7 = BookEventLog;
		formatter.Field(15, "BookEventLog", ref value7, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DialogState>();
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
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				SelectedAnswers = formatter.ReadPackable<HashSet<string>>(state);
				break;
			case 11:
				AnswerChecks = formatter.ReadPackable<Dictionary<string, CheckResult>>(state);
				break;
			case 12:
				ShownAnswerLists = formatter.ReadPackable<HashSet<string>>(state);
				break;
			case 13:
				ShownCues = formatter.ReadPackable<HashSet<string>>(state);
				break;
			case 14:
				ShownDialogs = formatter.ReadPackable<HashSet<string>>(state);
				break;
			case 15:
				BookEventLog = formatter.ReadPackable<Dictionary<BlueprintDialog, List<BlueprintScriptableObject>>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
