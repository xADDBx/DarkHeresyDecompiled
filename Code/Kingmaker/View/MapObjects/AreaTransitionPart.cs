using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[OwlPackable(OwlPackableMode.Generate)]
public class AreaTransitionPart : ViewBasedPart<AreaTransitionSettings>, IUnlockableFlagReference, IAreaEnterPointReference, IUnlockHandler, ISubscriber, IEtudesUpdateHandler, IAreaHandler, ILocalizationHandler, IHashable, IOwlPackable<AreaTransitionPart>
{
	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public bool AlreadyUnlocked;

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AreaTransitionPart",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("AlreadyUnlocked", typeof(bool))
		}
	};

	public BlueprintAreaEnterPoint AreaEnterPoint => base.Settings.AreaEnterPoint;

	public float ProximityDistance => base.Settings.ProximityDistance;

	public BlueprintAreaTransition Blueprint => base.Settings.Blueprint;

	protected override void OnSettingsDidSet(bool isNewSettings)
	{
		if (base.Settings.AddMapMarker)
		{
			LocalMapMarkerPart orCreate = base.ConcreteOwner.GetOrCreate<LocalMapMarkerPart>();
			orCreate.IsRuntimeCreated = true;
			orCreate.Settings.Type = LocalMapMarkType.Exit;
			if (AreaEnterPoint != null)
			{
				orCreate.NonLocalizedDescription = AreaEnterPoint.Tooltip(base.Settings.TooltipIndex);
			}
		}
		UpdateVisibility();
	}

	public void UpdateVisibility()
	{
		if (base.Settings.VisibilityFlag != null)
		{
			base.Owner.IsInGame = base.Settings.VisibilityFlag.IsUnlocked;
		}
		else if (base.Settings.VisibilityEtude != null)
		{
			Etude etude = Game.Instance.EtudesSystem.Etudes.Get(base.Settings.VisibilityEtude);
			base.Owner.IsInGame = etude?.IsPlaying ?? false;
		}
	}

	public bool CheckRestrictions(BaseUnitEntity user)
	{
		if (AlreadyUnlocked)
		{
			return true;
		}
		InteractionRestrictionPart interactionRestrictionPart = base.ConcreteOwner.GetAll<InteractionRestrictionPart>().FirstOrDefault();
		if (interactionRestrictionPart == null)
		{
			AlreadyUnlocked = true;
			return true;
		}
		foreach (InteractionRestrictionPart item in base.ConcreteOwner.Parts.GetAll<InteractionRestrictionPart>())
		{
			if (item.CheckRestriction(user))
			{
				item.ShowSuccessBark(user);
				AlreadyUnlocked = true;
				return true;
			}
		}
		interactionRestrictionPart.ShowRestrictionBark(user);
		return false;
	}

	public UnlockableFlagReferenceType GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		if (flag != base.Settings.VisibilityFlag)
		{
			return UnlockableFlagReferenceType.None;
		}
		return UnlockableFlagReferenceType.Check;
	}

	public bool GetUsagesFor(BlueprintAreaEnterPoint point)
	{
		return point == base.Settings.AreaEnterPoint;
	}

	public void HandleUnlock(BlueprintUnlockableFlag flag)
	{
		UpdateVisibility();
	}

	public void HandleLock(BlueprintUnlockableFlag flag)
	{
		UpdateVisibility();
	}

	public void OnEtudesUpdate()
	{
		UpdateVisibility();
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		UpdateVisibility();
	}

	public void HandleLanguageChanged()
	{
		if (base.Settings.AddMapMarker)
		{
			LocalMapMarkerPart orCreate = base.ConcreteOwner.GetOrCreate<LocalMapMarkerPart>();
			if (AreaEnterPoint != null)
			{
				orCreate.NonLocalizedDescription = AreaEnterPoint.Tooltip(base.Settings.TooltipIndex);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AreaTransitionPart source = new AreaTransitionPart();
		result = Unsafe.As<AreaTransitionPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AreaTransitionPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.UnmanagedField(1, "AlreadyUnlocked", ref AlreadyUnlocked, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AreaTransitionPart>();
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
				AlreadyUnlocked = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
