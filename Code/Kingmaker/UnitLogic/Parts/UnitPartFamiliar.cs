using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Critters;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartFamiliar : EntityPart<AbstractUnitEntity>, IHashable, IOwlPackable<UnitPartFamiliar>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<BaseUnitEntity> m_Leader;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartFamiliar",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Leader", typeof(EntityRef<BaseUnitEntity>))
		}
	};

	private FamiliarSettings FamiliarSettings => base.Owner.Blueprint.GetComponent<FamiliarSettingsOverride>()?.FamiliarSettings ?? ConfigRoot.Instance.FamiliarsRoot.DefaultFamiliarSettings;

	public BaseUnitEntity Leader => m_Leader.Entity;

	public bool IsLeaderVisible
	{
		get
		{
			if (Leader != null && Leader.View.IsVisible && Leader.IsViewActive)
			{
				return !Leader.IsInvisible;
			}
			return false;
		}
	}

	public bool IsVisible
	{
		get
		{
			if (IsLeaderVisible)
			{
				return !NeedHide();
			}
			return false;
		}
	}

	private bool NeedHide()
	{
		if (Leader == null)
		{
			return true;
		}
		HideFamiliarSettings hideFamiliarSettings = FamiliarSettings.HideFamiliarSettings;
		if (hideFamiliarSettings.HideInCapitalMode && Game.Instance.LoadedAreaState.Settings.CapitalPartyMode)
		{
			return true;
		}
		if (hideFamiliarSettings.HideInCombat && Leader.IsInCombat)
		{
			return true;
		}
		if (hideFamiliarSettings.HideInStealth && Leader.Stealth.Active)
		{
			return true;
		}
		if (hideFamiliarSettings.HideIfLeaderUnconscious)
		{
			return !Leader.LifeState.IsConscious;
		}
		return false;
	}

	public void Init(BaseUnitEntity leader)
	{
		m_Leader = leader;
		OnAttachOrPostLoad();
		OnViewDidAttach();
	}

	protected override void OnAttachOrPostLoad()
	{
		base.OnAttachOrPostLoad();
		if (Leader != null)
		{
			base.Owner.Features.IsUntargetable.Retain();
			base.Owner.Features.IsIgnoredByCombat.Retain();
			base.Owner.SetViewHandlingOnDisposePolicy(FamiliarSettings.ViewHandlingOnDisposePolicyType);
			if (base.Owner.GetOptional<PartDetectiveServoSkull>() == null)
			{
				base.Owner.GetOrCreate<UnitPartFollowUnit>().Init(Leader, FamiliarSettings.FollowerSettings);
			}
		}
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		if (Leader != null)
		{
			base.Owner.View.EnsureComponent<FamiliarUnit>().TeleportToLeader();
			UpdateViewVisibility();
		}
	}

	public void UpdateViewVisibility()
	{
		base.Owner.View?.SetVisible(IsVisible);
	}

	public void UpdateIsInGameState(bool isInGame)
	{
		base.Owner.IsInGame = isInGame;
	}

	protected override void OnApplyPostLoadFixes()
	{
		UnitPartFamiliarLeader unitPartFamiliarLeader = Leader?.GetFamiliarLeaderOptional();
		if (unitPartFamiliarLeader == null || !unitPartFamiliarLeader.HasEquippedFamiliar(base.Owner))
		{
			Game.Instance.Controllers.EntityDestroyer.Destroy(base.Owner);
			PFLog.Default.Warning($"Invalid familiar removed: {base.Owner}");
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityRef<BaseUnitEntity> obj = m_Leader;
		Hash128 val2 = StructHasher<EntityRef<BaseUnitEntity>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartFamiliar source = new UnitPartFamiliar();
		result = Unsafe.As<UnitPartFamiliar, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartFamiliar>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Leader", ref m_Leader, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartFamiliar>();
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
				m_Leader = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
