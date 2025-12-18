using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Settings;

[OwlPackable(OwlPackableMode.Generate)]
public class AutoPauseValues : IOwlPackable, IOwlPackable<AutoPauseValues>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AutoPauseValues",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("PauseOnLostFocus", typeof(bool)),
			new FieldInfo("PauseOnTrapDetected", typeof(bool)),
			new FieldInfo("PauseOnHiddenObjectDetected", typeof(bool)),
			new FieldInfo("PauseOnAreaLoaded", typeof(bool)),
			new FieldInfo("PauseOnLoadingScreen", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool PauseOnLostFocus { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool PauseOnTrapDetected { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool PauseOnHiddenObjectDetected { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool PauseOnAreaLoaded { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool PauseOnLoadingScreen { get; set; }

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AutoPauseValues source = new AutoPauseValues();
		result = Unsafe.As<AutoPauseValues, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AutoPauseValues>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		bool value = PauseOnLostFocus;
		formatter.UnmanagedField(0, "PauseOnLostFocus", ref value, state);
		bool value2 = PauseOnTrapDetected;
		formatter.UnmanagedField(1, "PauseOnTrapDetected", ref value2, state);
		bool value3 = PauseOnHiddenObjectDetected;
		formatter.UnmanagedField(2, "PauseOnHiddenObjectDetected", ref value3, state);
		bool value4 = PauseOnAreaLoaded;
		formatter.UnmanagedField(3, "PauseOnAreaLoaded", ref value4, state);
		bool value5 = PauseOnLoadingScreen;
		formatter.UnmanagedField(4, "PauseOnLoadingScreen", ref value5, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AutoPauseValues>();
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
				PauseOnLostFocus = formatter.ReadUnmanaged<bool>(state);
				break;
			case 1:
				PauseOnTrapDetected = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				PauseOnHiddenObjectDetected = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				PauseOnAreaLoaded = formatter.ReadUnmanaged<bool>(state);
				break;
			case 4:
				PauseOnLoadingScreen = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
