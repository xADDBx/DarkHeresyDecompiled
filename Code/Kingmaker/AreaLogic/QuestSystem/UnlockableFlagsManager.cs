using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AreaLogic.QuestSystem;

[HashRoot]
[OwlPackable(OwlPackableMode.Generate)]
public class UnlockableFlagsManager : IHashable, IOwlPackable, IOwlPackable<UnlockableFlagsManager>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnlockableFlagsManager",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_UnlockedFlags", typeof(Dictionary<BlueprintUnlockableFlag, int>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private Dictionary<BlueprintUnlockableFlag, int> m_UnlockedFlags { get; set; } = new Dictionary<BlueprintUnlockableFlag, int>();


	public Dictionary<BlueprintUnlockableFlag, int> UnlockedFlags => m_UnlockedFlags;

	private IEnumerable<string> UnlocksInStateExplorer => m_UnlockedFlags.Select((KeyValuePair<BlueprintUnlockableFlag, int> p) => p.Key.name + ": " + p.Value);

	public void Lock(BlueprintUnlockableFlag flag)
	{
		m_UnlockedFlags.Remove(flag);
		EventBus.RaiseEvent(delegate(IUnlockHandler h)
		{
			h.HandleLock(flag);
		});
	}

	public void Unlock(BlueprintUnlockableFlag flag)
	{
		if (!IsUnlocked(flag))
		{
			m_UnlockedFlags.Add(flag, 0);
			EventBus.RaiseEvent(delegate(IUnlockHandler h)
			{
				h.HandleUnlock(flag);
			});
		}
	}

	public void SetFlagValue(BlueprintUnlockableFlag flag, int value)
	{
		bool num = !m_UnlockedFlags.ContainsKey(flag);
		m_UnlockedFlags[flag] = value;
		if (num)
		{
			EventBus.RaiseEvent(delegate(IUnlockHandler h)
			{
				h.HandleUnlock(flag);
			});
		}
		EventBus.RaiseEvent(delegate(IUnlockValueHandler h)
		{
			h.HandleFlagValue(flag, value);
		});
	}

	public int GetFlagValue(BlueprintUnlockableFlag flag)
	{
		m_UnlockedFlags.TryGetValue(flag, out var value);
		return value;
	}

	public bool IsUnlocked(BlueprintUnlockableFlag flag)
	{
		return m_UnlockedFlags.ContainsKey(flag);
	}

	public bool IsLocked(BlueprintUnlockableFlag flag)
	{
		return !m_UnlockedFlags.ContainsKey(flag);
	}

	public override string ToString()
	{
		return "Unlocks";
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Dictionary<BlueprintUnlockableFlag, int> unlockedFlags = m_UnlockedFlags;
		if (unlockedFlags != null)
		{
			int val = 0;
			foreach (KeyValuePair<BlueprintUnlockableFlag, int> item in unlockedFlags)
			{
				Hash128 hash = default(Hash128);
				Hash128 val2 = SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val2);
				int obj = item.Value;
				Hash128 val3 = UnmanagedHasher<int>.GetHash128(ref obj);
				hash.Append(ref val3);
				val ^= hash.GetHashCode();
			}
			result.Append(ref val);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnlockableFlagsManager source = new UnlockableFlagsManager();
		result = Unsafe.As<UnlockableFlagsManager, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<UnlockableFlagsManager>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Dictionary<BlueprintUnlockableFlag, int> value = m_UnlockedFlags;
		formatter.Field(0, "m_UnlockedFlags", ref value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnlockableFlagsManager>();
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
				m_UnlockedFlags = formatter.ReadPackable<Dictionary<BlueprintUnlockableFlag, int>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
