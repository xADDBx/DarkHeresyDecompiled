using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Settings.Difficulty;

[OwlPackable(OwlPackableMode.Generate)]
public class MinDifficultyController : IHashable, IOwlPackable, IOwlPackable<MinDifficultyController>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MinDifficultyController",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("MinDifficulty", typeof(DifficultyPreset))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public DifficultyPreset MinDifficulty { get; private set; }

	public void UpdateMinDifficulty(bool force = false)
	{
		DifficultyPreset difficultyPreset = SettingsController.Instance.DifficultySettingsController.ExtractFromSettings();
		if (force || MinDifficulty == null || difficultyPreset.CompareTo(MinDifficulty) < 0)
		{
			MinDifficulty = difficultyPreset;
		}
	}

	public void PostLoad()
	{
		UpdateMinDifficulty();
	}

	public void PreSave()
	{
		UpdateMinDifficulty();
	}

	public void ResetMinDifficulty()
	{
		MinDifficulty = SettingsController.Instance.DifficultySettingsController.ExtractFromSettings();
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<DifficultyPreset>.GetHash128(MinDifficulty);
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MinDifficultyController source = new MinDifficultyController();
		result = Unsafe.As<MinDifficultyController, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MinDifficultyController>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		DifficultyPreset value = MinDifficulty;
		formatter.Field(0, "MinDifficulty", ref value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MinDifficultyController>();
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
				MinDifficulty = formatter.ReadPackable<DifficultyPreset>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
