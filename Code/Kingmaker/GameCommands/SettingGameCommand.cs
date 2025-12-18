using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Networking.Settings;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class SettingGameCommand : GameCommand, IOwlPackable<SettingGameCommand>
{
	[JsonProperty]
	private readonly List<BaseSettingNetData> m_Settings;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SettingGameCommand",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SettingGameCommand()
	{
	}

	public SettingGameCommand(List<BaseSettingNetData> m_settings)
	{
		m_Settings = m_settings;
	}

	protected override void ExecuteInternal()
	{
		SettingsController.Instance.RevertAllTempValues();
		foreach (BaseSettingNetData setting in m_Settings)
		{
			setting.ForceSet();
		}
		Game.Instance.UISettingsManager.OnSettingsApplied();
		EventBus.RaiseEvent(delegate(ISaveSettingsHandler h)
		{
			h.HandleSaveSettings();
		});
		SettingsController.Instance.ConfirmAllTempValues();
		SettingsController.Instance.SaveAll();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SettingGameCommand source = new SettingGameCommand();
		result = Unsafe.As<SettingGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SettingGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SettingGameCommand>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
