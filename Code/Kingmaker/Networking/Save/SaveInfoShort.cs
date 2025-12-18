using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Networking.Save;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
public readonly struct SaveInfoShort : IOwlPackable, IOwlPackable<SaveInfoShort>
{
	[JsonProperty]
	[OwlPackInclude]
	public readonly string Name;

	[JsonProperty]
	[OwlPackInclude]
	public readonly BlueprintArea Area;

	[JsonProperty]
	[OwlPackInclude]
	public readonly string AreaNameOverride;

	[JsonProperty]
	[OwlPackInclude]
	public readonly List<PortraitForSave> PartyPortraits;

	[JsonProperty]
	[OwlPackInclude]
	public readonly DateTime SystemSaveTime;

	[JsonProperty]
	[OwlPackInclude]
	public readonly TimeSpan GameTotalTime;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SaveInfoShort",
		Fields = new FieldInfo[6]
		{
			new FieldInfo("Name", typeof(string)),
			new FieldInfo("Area", typeof(BlueprintArea)),
			new FieldInfo("AreaNameOverride", typeof(string)),
			new FieldInfo("PartyPortraits", typeof(List<PortraitForSave>)),
			new FieldInfo("SystemSaveTime", typeof(DateTime)),
			new FieldInfo("GameTotalTime", typeof(TimeSpan))
		}
	};

	[JsonIgnore]
	public bool IsEmpty => SystemSaveTime == default(DateTime);

	public SaveInfoShort(string name, BlueprintArea area, string areaNameOverride, List<PortraitForSave> partyPortraits, DateTime systemSaveTime, TimeSpan gameTotalTime)
	{
		Name = name;
		Area = area;
		AreaNameOverride = areaNameOverride;
		PartyPortraits = partyPortraits;
		SystemSaveTime = systemSaveTime;
		GameTotalTime = gameTotalTime;
	}

	public SaveInfoShort(SaveInfo saveInfo)
		: this(saveInfo.Name, saveInfo.Area, saveInfo.AreaNameOverride, saveInfo.PartyPortraits, saveInfo.SystemSaveTime, saveInfo.GameTotalTime)
	{
	}

	public static explicit operator SaveInfo(SaveInfoShort saveInfoShort)
	{
		return new SaveInfo
		{
			Name = saveInfoShort.Name,
			Area = saveInfoShort.Area,
			AreaNameOverride = saveInfoShort.AreaNameOverride,
			PartyPortraits = saveInfoShort.PartyPortraits,
			SystemSaveTime = saveInfoShort.SystemSaveTime,
			GameTotalTime = saveInfoShort.GameTotalTime
		};
	}

	public static explicit operator SaveInfoShort(SaveInfo saveInfo)
	{
		return new SaveInfoShort(saveInfo);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SaveInfoShort source = default(SaveInfoShort);
		result = Unsafe.As<SaveInfoShort, TPossiblyBase>(ref source);
	}

	public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<SaveInfoShort>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = Name;
		formatter.StringField(0, "Name", ref value, state);
		BlueprintArea value2 = Area;
		formatter.Field(1, "Area", ref value2, state);
		string value3 = AreaNameOverride;
		formatter.StringField(2, "AreaNameOverride", ref value3, state);
		List<PortraitForSave> value4 = PartyPortraits;
		formatter.Field(3, "PartyPortraits", ref value4, state);
		DateTime value5 = SystemSaveTime;
		formatter.Field(4, "SystemSaveTime", ref value5, state);
		TimeSpan value6 = GameTotalTime;
		formatter.Field(5, "GameTotalTime", ref value6, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SaveInfoShort>();
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
				Unsafe.AsRef(in Name) = formatter.ReadString(state);
				break;
			case 1:
				Unsafe.AsRef(in Area) = formatter.ReadPackable<BlueprintArea>(state);
				break;
			case 2:
				Unsafe.AsRef(in AreaNameOverride) = formatter.ReadString(state);
				break;
			case 3:
				Unsafe.AsRef(in PartyPortraits) = formatter.ReadPackable<List<PortraitForSave>>(state);
				break;
			case 4:
				Unsafe.AsRef(in SystemSaveTime) = formatter.ReadPackable<DateTime>(state);
				break;
			case 5:
				Unsafe.AsRef(in GameTotalTime) = formatter.ReadPackable<TimeSpan>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
