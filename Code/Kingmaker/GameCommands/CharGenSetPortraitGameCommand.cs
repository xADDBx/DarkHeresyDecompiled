using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenSetPortraitGameCommand : GameCommand, IOwlPackable<CharGenSetPortraitGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly BlueprintPortraitReference m_Blueprint;

	[JsonProperty]
	[OwlPackInclude]
	private readonly Guid m_CustomPortraitGuid;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenSetPortraitGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Blueprint", typeof(BlueprintPortraitReference)),
			new FieldInfo("m_CustomPortraitGuid", typeof(Guid))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private CharGenSetPortraitGameCommand([NotNull] BlueprintPortraitReference m_blueprint, Guid m_customPortraitGuid)
	{
		if (m_blueprint == null)
		{
			throw new ArgumentNullException("m_blueprint");
		}
		m_Blueprint = m_blueprint;
		m_CustomPortraitGuid = m_customPortraitGuid;
	}

	private CharGenSetPortraitGameCommand(OwlPackConstructorParameter _)
	{
	}

	public CharGenSetPortraitGameCommand([NotNull] BlueprintPortrait blueprint)
	{
		if (blueprint == null)
		{
			throw new ArgumentNullException("blueprint");
		}
		m_Blueprint = blueprint.ToReference<BlueprintPortraitReference>();
		PortraitData data = blueprint.Data;
		if (SavePacker.TryGetGuidFromPortrait(data, out m_CustomPortraitGuid))
		{
			PFLog.GameCommands.Log($"[CharGenSetPortraitGameCommand] [Create] CustomPortrait '{data.CustomId}' -> '{m_CustomPortraitGuid}'");
			if (PhotonManager.Initialized)
			{
				PhotonManager.Instance.PortraitSyncer.SetNewPortraitForSending(m_CustomPortraitGuid);
			}
		}
		else if (PhotonManager.Initialized)
		{
			PhotonManager.Instance.PortraitSyncer.ClearPortraitForSending();
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintPortrait blueprint = m_Blueprint;
		if (blueprint == null)
		{
			PFLog.GameCommands.Log("[CharGenSetPortraitGameCommand] BlueprintPortrait was not found id=" + m_Blueprint.Guid);
			return;
		}
		if (m_CustomPortraitGuid != Guid.Empty)
		{
			string portraitId;
			bool flag = SavePacker.TryGetPortraitIdFromGuid(m_CustomPortraitGuid, out portraitId);
			PFLog.GameCommands.Log($"[CharGenSetPortraitGameCommand] [Execute] CustomPortrait '{m_CustomPortraitGuid}' -> '{portraitId}' found={flag}");
			if (PhotonManager.Initialized)
			{
				PhotonManager.Instance.PortraitSyncer.SetPortraitForReceiving(m_CustomPortraitGuid, blueprint, flag);
				if (!flag)
				{
					return;
				}
			}
			blueprint.Data = new PortraitData(portraitId);
		}
		EventBus.RaiseEvent(delegate(ICharGenPortraitHandler h)
		{
			h.HandleSetPortrait(blueprint);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetPortraitGameCommand source = new CharGenSetPortraitGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetPortraitGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetPortraitGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintPortraitReference value = m_Blueprint;
		formatter.Field(0, "m_Blueprint", ref value, state);
		Guid value2 = m_CustomPortraitGuid;
		formatter.Field(1, "m_CustomPortraitGuid", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetPortraitGameCommand>();
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
				Unsafe.AsRef(in m_Blueprint) = formatter.ReadPackable<BlueprintPortraitReference>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_CustomPortraitGuid) = formatter.ReadPackable<Guid>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
