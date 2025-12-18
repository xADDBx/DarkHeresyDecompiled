using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class LoadAreaGameCommand : GameCommand, IOwlPackable<LoadAreaGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly BlueprintAreaEnterPointReference m_EnterPoint;

	[JsonProperty]
	[OwlPackInclude]
	private readonly AutoSaveMode m_AutoSaveMode;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "LoadAreaGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_EnterPoint", typeof(BlueprintAreaEnterPointReference)),
			new FieldInfo("m_AutoSaveMode", typeof(AutoSaveMode))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public LoadAreaGameCommand([NotNull] BlueprintAreaEnterPointReference m_enterPoint, AutoSaveMode m_autoSaveMode)
	{
		m_EnterPoint = m_enterPoint;
		m_AutoSaveMode = m_autoSaveMode;
		if (m_EnterPoint == null)
		{
			throw new ArgumentNullException("m_enterPoint");
		}
		if ((BlueprintAreaEnterPoint)m_EnterPoint == null)
		{
			throw new NullReferenceException("EnterPoint was not found! " + m_EnterPoint.Guid);
		}
	}

	private LoadAreaGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		BlueprintAreaEnterPoint blueprintAreaEnterPoint = m_EnterPoint;
		if (blueprintAreaEnterPoint == null)
		{
			PFLog.GameCommands.Log("[LoadAreaGameCommand] EnterPoint was not found! " + m_EnterPoint.Guid);
		}
		else
		{
			Game.Instance.LoadArea(blueprintAreaEnterPoint, m_AutoSaveMode);
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		LoadAreaGameCommand source = new LoadAreaGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<LoadAreaGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<LoadAreaGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintAreaEnterPointReference value = m_EnterPoint;
		formatter.Field(0, "m_EnterPoint", ref value, state);
		AutoSaveMode value2 = m_AutoSaveMode;
		formatter.EnumField(1, "m_AutoSaveMode", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<LoadAreaGameCommand>();
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
				Unsafe.AsRef(in m_EnterPoint) = formatter.ReadPackable<BlueprintAreaEnterPointReference>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_AutoSaveMode) = formatter.ReadEnum<AutoSaveMode>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
