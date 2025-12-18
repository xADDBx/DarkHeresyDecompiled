using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Progression.Paths;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenSelectCareerPathGameCommand : GameCommand, IOwlPackable<CharGenSelectCareerPathGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly BlueprintCareerPathReference m_CareerPathRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenSelectCareerPathGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_CareerPathRef", typeof(BlueprintCareerPathReference))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private CharGenSelectCareerPathGameCommand([NotNull] BlueprintCareerPathReference m_careerPathRef)
	{
		m_CareerPathRef = m_careerPathRef;
	}

	private CharGenSelectCareerPathGameCommand(OwlPackConstructorParameter _)
	{
	}

	public CharGenSelectCareerPathGameCommand([NotNull] BlueprintCareerPath careerPath)
		: this(careerPath.ToReference<BlueprintCareerPathReference>())
	{
		if (careerPath == null)
		{
			throw new ArgumentNullException("careerPath");
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintCareerPath blueprintCareerPath = m_CareerPathRef.Get();
		if (blueprintCareerPath == null)
		{
			PFLog.GameCommands.Error("[CharGenSelectCareerPathGameCommand] BlueprintCareerPath not found #" + m_CareerPathRef.Guid);
			return;
		}
		EventBus.RaiseEvent(delegate(ICharGenCareerPathHandler h)
		{
			h.HandleCareerPath(blueprintCareerPath);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSelectCareerPathGameCommand source = new CharGenSelectCareerPathGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSelectCareerPathGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSelectCareerPathGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintCareerPathReference value = m_CareerPathRef;
		formatter.Field(0, "m_CareerPathRef", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSelectCareerPathGameCommand>();
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
				Unsafe.AsRef(in m_CareerPathRef) = formatter.ReadPackable<BlueprintCareerPathReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
