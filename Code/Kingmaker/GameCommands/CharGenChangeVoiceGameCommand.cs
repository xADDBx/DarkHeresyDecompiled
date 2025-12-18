using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Visual.Sound;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenChangeVoiceGameCommand : GameCommand, IOwlPackable<CharGenChangeVoiceGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly BlueprintUnitAsksListReference m_Blueprint;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenChangeVoiceGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Blueprint", typeof(BlueprintUnitAsksListReference))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private CharGenChangeVoiceGameCommand([NotNull] BlueprintUnitAsksListReference m_blueprint)
	{
		if (m_blueprint == null)
		{
			throw new ArgumentNullException("m_blueprint");
		}
		m_Blueprint = m_blueprint;
	}

	private CharGenChangeVoiceGameCommand(OwlPackConstructorParameter _)
	{
	}

	public CharGenChangeVoiceGameCommand([NotNull] BlueprintUnitAsksList blueprint)
		: this(blueprint.ToReference<BlueprintUnitAsksListReference>())
	{
		if (blueprint == null)
		{
			throw new ArgumentNullException("blueprint");
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintUnitAsksList blueprint = m_Blueprint;
		if (blueprint == null)
		{
			PFLog.GameCommands.Log("[CharGenChangeVoiceGameCommand] BlueprintUnitAsksList was not found id=" + m_Blueprint.Guid);
			return;
		}
		EventBus.RaiseEvent(delegate(ICharGenAppearancePhaseVoiceHandler h)
		{
			h.HandleChangeVoice(blueprint);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenChangeVoiceGameCommand source = new CharGenChangeVoiceGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenChangeVoiceGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenChangeVoiceGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintUnitAsksListReference value = m_Blueprint;
		formatter.Field(0, "m_Blueprint", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenChangeVoiceGameCommand>();
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
				Unsafe.AsRef(in m_Blueprint) = formatter.ReadPackable<BlueprintUnitAsksListReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
