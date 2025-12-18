using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using OwlPack.Runtime;

namespace Kingmaker.Networking;

[OwlPackable(OwlPackableMode.Generate)]
public struct RequestBugReportMessage : IOwlPackable, IOwlPackable<RequestBugReportMessage>
{
	[OwlPackInclude]
	public readonly string Id;

	[OwlPackInclude]
	public readonly string Text;

	[OwlPackInclude]
	public readonly string Type;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "RequestBugReportMessage",
		Fields = new FieldInfo[3]
		{
			new FieldInfo("Id", typeof(string)),
			new FieldInfo("Text", typeof(string)),
			new FieldInfo("Type", typeof(string))
		}
	};

	public RequestBugReportMessage([NotNull] string id, [CanBeNull] string text, [CanBeNull] string type)
	{
		Id = id;
		Text = text;
		Type = type;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		RequestBugReportMessage source = default(RequestBugReportMessage);
		result = Unsafe.As<RequestBugReportMessage, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<RequestBugReportMessage>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = Id;
		formatter.StringField(0, "Id", ref value, state);
		string value2 = Text;
		formatter.StringField(1, "Text", ref value2, state);
		string value3 = Type;
		formatter.StringField(2, "Type", ref value3, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RequestBugReportMessage>();
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
				Unsafe.AsRef(in Id) = formatter.ReadString(state);
				break;
			case 1:
				Unsafe.AsRef(in Text) = formatter.ReadString(state);
				break;
			case 2:
				Unsafe.AsRef(in Type) = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
