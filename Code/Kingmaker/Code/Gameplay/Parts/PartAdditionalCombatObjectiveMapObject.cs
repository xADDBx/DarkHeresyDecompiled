using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using OwlPack.Runtime;
using R3;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Parts;

[OwlPackOldName("Kingmaker.Code.Gameplay.Parts.PartAdditionalCombatObjective")]
[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartAdditionalCombatObjectiveMapObject : EntityPartWithConfig<AdditionalCombatObjectiveSettings>, IHashable, IOwlPackable<PartAdditionalCombatObjectiveMapObject>
{
	private bool m_IsLoaded;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartAdditionalCombatObjectiveMapObject",
		OldNames = new string[1] { "Kingmaker.Code.Gameplay.Parts.PartAdditionalCombatObjective" },
		Fields = new FieldInfo[3]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("WasHovered", typeof(bool)),
			new FieldInfo("ShowType", typeof(AdditionalCombatType))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool WasHovered { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public AdditionalCombatType ShowType { get; private set; }

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		m_IsLoaded = true;
	}

	protected override void OnConfigDidSet(bool isNewConfig)
	{
		base.OnConfigDidSet(isNewConfig);
		if (!m_IsLoaded)
		{
			ShowType = base.Settings.ShowType;
		}
		IEntityView view = base.View;
		MapObjectView mapObjectView = view as MapObjectView;
		if ((object)mapObjectView == null)
		{
			return;
		}
		if (ShowType == AdditionalCombatType.Always)
		{
			mapObjectView.SetSilentHighlight(value: true);
		}
		GameUIState.Instance.IsInCombat.Subscribe(delegate(bool value)
		{
			switch (ShowType)
			{
			case AdditionalCombatType.InCombat:
				mapObjectView.SetSilentHighlight(value);
				break;
			case AdditionalCombatType.OnceInCombat:
				mapObjectView.SetSilentHighlight(value && !WasHovered);
				break;
			}
		});
	}

	public bool ShouldHighlight()
	{
		switch (ShowType)
		{
		case AdditionalCombatType.Always:
			return base.View.Data.IsRevealed;
		case AdditionalCombatType.InCombat:
		case AdditionalCombatType.OnceInCombat:
			if (base.View.Data.IsRevealed)
			{
				return GameUIState.Instance.IsInCombat.Value;
			}
			return false;
		case AdditionalCombatType.Never:
			return false;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void SetHovered(bool isHovered)
	{
		if (isHovered)
		{
			WasHovered = GameUIState.Instance.IsInCombat.Value;
		}
		else if (base.View is MapObjectView mapObjectView && ShowType == AdditionalCombatType.OnceInCombat)
		{
			mapObjectView.SetSilentHighlight(value: false);
		}
	}

	public void SetShowType(AdditionalCombatType showType)
	{
		ShowType = showType;
		if (base.View is MapObjectView mapObjectView)
		{
			switch (ShowType)
			{
			case AdditionalCombatType.Always:
				mapObjectView.SetSilentHighlight(value: true);
				break;
			case AdditionalCombatType.InCombat:
				mapObjectView.SetSilentHighlight(GameUIState.Instance.IsInCombat.Value);
				break;
			case AdditionalCombatType.OnceInCombat:
				mapObjectView.SetSilentHighlight(GameUIState.Instance.IsInCombat.Value && !WasHovered);
				break;
			case AdditionalCombatType.Never:
				mapObjectView.SetSilentHighlight(value: false);
				break;
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = WasHovered;
		result.Append(ref val2);
		AdditionalCombatType val3 = ShowType;
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartAdditionalCombatObjectiveMapObject source = new PartAdditionalCombatObjectiveMapObject();
		result = Unsafe.As<PartAdditionalCombatObjectiveMapObject, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAdditionalCombatObjectiveMapObject>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = WasHovered;
		formatter.UnmanagedField(1, "WasHovered", ref value2, state);
		AdditionalCombatType value3 = ShowType;
		formatter.EnumField(2, "ShowType", ref value3, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAdditionalCombatObjectiveMapObject>();
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
				base.SourceType = formatter.ReadString(state);
				break;
			case 1:
				WasHovered = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				ShowType = formatter.ReadEnum<AdditionalCombatType>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
