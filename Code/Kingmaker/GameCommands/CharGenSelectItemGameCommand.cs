using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenSelectItemGameCommand : GameCommand, IOwlPackable<CharGenSelectItemGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly FeatureGroup m_FeatureGroup;

	[JsonProperty]
	[OwlPackInclude]
	private readonly BlueprintFeatureReference m_BlueprintFeature;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenSelectItemGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_FeatureGroup", typeof(FeatureGroup)),
			new FieldInfo("m_BlueprintFeature", typeof(BlueprintFeatureReference))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private CharGenSelectItemGameCommand(FeatureGroup m_featureGroup, [NotNull] BlueprintFeatureReference m_blueprintFeature)
	{
		if (m_blueprintFeature == null)
		{
			throw new ArgumentNullException("m_blueprintFeature");
		}
		m_FeatureGroup = m_featureGroup;
		m_BlueprintFeature = m_blueprintFeature;
	}

	private CharGenSelectItemGameCommand(OwlPackConstructorParameter _)
	{
	}

	public CharGenSelectItemGameCommand(FeatureGroup featureGroup, [NotNull] BlueprintFeature feature)
		: this(featureGroup, feature.ToReference<BlueprintFeatureReference>())
	{
		if (feature == null)
		{
			throw new ArgumentNullException("feature");
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintFeature blueprintFeature = m_BlueprintFeature.Get();
		if (blueprintFeature == null)
		{
			PFLog.GameCommands.Error("[CharGenSelectItemGameCommand] BlueprintFeature not found #" + m_BlueprintFeature.Guid);
			return;
		}
		EventBus.RaiseEvent(delegate(ICharGenSelectItemHandler h)
		{
			h.HandleSelectItem(m_FeatureGroup, blueprintFeature);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSelectItemGameCommand source = new CharGenSelectItemGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSelectItemGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSelectItemGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		FeatureGroup value = m_FeatureGroup;
		formatter.EnumField(0, "m_FeatureGroup", ref value, state);
		BlueprintFeatureReference value2 = m_BlueprintFeature;
		formatter.Field(1, "m_BlueprintFeature", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSelectItemGameCommand>();
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
				Unsafe.AsRef(in m_FeatureGroup) = formatter.ReadEnum<FeatureGroup>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_BlueprintFeature) = formatter.ReadPackable<BlueprintFeatureReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
