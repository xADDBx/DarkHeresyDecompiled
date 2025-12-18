using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using R3;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Parts.ViewBased;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartDetectiveObject : ViewBasedPart<DetectiveObjectSettings>, IHashable, IOwlPackable<PartDetectiveObject>
{
	private bool m_IsLoaded;

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartDetectiveObject",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("IsRevealed", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool IsRevealed { get; private set; }

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		m_IsLoaded = true;
	}

	protected override void OnSettingsDidSet(bool isNewSettings)
	{
		base.OnSettingsDidSet(isNewSettings);
		bool revealed = (m_IsLoaded ? IsRevealed : base.Settings.DefaultRevealedState);
		SetRevealed(revealed);
		GameUIState.Instance.GameMode.Subscribe(DoStartMode).AddTo(base.View.GO);
	}

	public void SetRevealed(bool isRevealed)
	{
		IsRevealed = isRevealed;
		UpdateVisual(isRevealed);
		if (isRevealed)
		{
			CreateMarkers();
		}
		else
		{
			RemoveMarkers();
		}
	}

	private void UpdateVisual(bool isRevealed)
	{
		base.Settings.GroundHighlight.Or(null)?.gameObject.SetActive(isRevealed);
		(base.View as MapObjectView)?.ForceHighlightExternal(isRevealed);
	}

	private void CreateMarkers()
	{
		if (base.Settings.LocalMapMarker != 0 && base.View.Data is MechanicEntity mechanicEntity)
		{
			LocalMapMarkerPart orCreate = mechanicEntity.GetOrCreate<LocalMapMarkerPart>();
			orCreate.IsRuntimeCreated = true;
			LocalMapMarkerSettings settings = orCreate.Settings;
			settings.Type = base.Settings.LocalMapMarker switch
			{
				DetectiveObjectSettings.LocalMapMarkerType.Lens => LocalMapMarkType.DetectiveSignal, 
				DetectiveObjectSettings.LocalMapMarkerType.Traces => LocalMapMarkType.DetectiveTrace, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}

	private void RemoveMarkers()
	{
		if (base.View.Data is MechanicEntity mechanicEntity)
		{
			mechanicEntity.Remove<LocalMapMarkerPart>();
		}
	}

	public void DoStartMode(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			if (Game.Instance.EntityPools.Cutscenes.Any((CutscenePlayerData c) => c.HasActiveLockControl))
			{
				UpdateVisual(isRevealed: false);
			}
		}
		else
		{
			UpdateVisual(IsRevealed);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = IsRevealed;
		result.Append(ref val2);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartDetectiveObject source = new PartDetectiveObject();
		result = Unsafe.As<PartDetectiveObject, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartDetectiveObject>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = IsRevealed;
		formatter.UnmanagedField(1, "IsRevealed", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartDetectiveObject>();
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
				IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
