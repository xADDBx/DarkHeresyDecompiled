using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.FlagCountable;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[Serializable]
[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartFlashlight : BaseUnitPart, IHashable, IOwlPackable<UnitPartFlashlight>
{
	[OwlPackInclude]
	[Obsolete]
	private readonly CountableFlag m_FlashlightEnabled = new CountableFlag();

	[OwlPackInclude]
	private bool m_Enabled;

	[OwlPackInclude]
	private int m_PreviousFormation;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartFlashlight",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("m_FlashlightEnabled", typeof(CountableFlag)),
			new FieldInfo("m_Enabled", typeof(bool)),
			new FieldInfo("ForcedLookAtPosition", typeof(Vector3?)),
			new FieldInfo("FlashlightInUse", typeof(bool)),
			new FieldInfo("m_PreviousFormation", typeof(int))
		}
	};

	private KingmakerEquipmentEntityReference m_Flashlight => ConfigRoot.Instance.CharGenRoot.Flashlight;

	public bool IsFlashlightEnabled => m_Enabled;

	[OwlPackInclude]
	public Vector3? ForcedLookAtPosition { get; set; }

	[OwlPackInclude]
	public bool FlashlightInUse { get; set; }

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		Gender gender = base.Owner.Gender;
		Race race = base.Owner.Blueprint.Race?.RaceId ?? Race.Human;
		if (base.Owner.View.CharacterAvatar != null)
		{
			base.Owner.View.CharacterAvatar.AddEquipmentEntities(m_Flashlight.Get().GetLinks(gender, race));
		}
	}

	protected override void OnViewWillDetach()
	{
		Gender gender = base.Owner.Gender;
		Race race = base.Owner.Blueprint.Race?.RaceId ?? Race.Human;
		if (base.Owner.View.CharacterAvatar != null)
		{
			base.Owner.View.CharacterAvatar.AddEquipmentEntities(m_Flashlight.Get().GetLinks(gender, race));
		}
		base.OnViewWillDetach();
	}

	public static bool IsOwner(BaseUnitEntity unit)
	{
		return GetFromOwner(unit) != null;
	}

	public static UnitPartFlashlight? GetFromOwner(BaseUnitEntity unit)
	{
		return unit.GetOptional<UnitPartFlashlight>();
	}

	public void FlashlightOn()
	{
		m_Enabled = true;
		RetainFlashlightFormation();
	}

	public void FlashlightOff()
	{
		m_Enabled = false;
		ReleaseFlashlightFormation();
	}

	[OwlPackOnAfterDeserialize]
	private void OnAfterDeserialize()
	{
		CountableFlag flashlightEnabled = m_FlashlightEnabled;
		if (flashlightEnabled != null && flashlightEnabled.Value)
		{
			m_Enabled = true;
			m_FlashlightEnabled.ReleaseAll();
		}
	}

	private void RetainFlashlightFormation()
	{
		m_PreviousFormation = Game.Instance.Player.FormationManager.CurrentFormationIndex;
		Game.Instance.GameCommandQueue.PartyFormationIndex(Game.Instance.Player.FormationManager.FlashlightFormationIndex);
	}

	private void ReleaseFlashlightFormation()
	{
		Game.Instance.GameCommandQueue.PartyFormationIndex(m_PreviousFormation);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartFlashlight source = new UnitPartFlashlight();
		result = Unsafe.As<UnitPartFlashlight, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartFlashlight>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		CountableFlag value = m_FlashlightEnabled;
		formatter.Field(0, "m_FlashlightEnabled", ref value, state);
		formatter.UnmanagedField(1, "m_Enabled", ref m_Enabled, state);
		Vector3? value2 = ForcedLookAtPosition;
		formatter.NullableField(2, "ForcedLookAtPosition", ref value2, state);
		bool value3 = FlashlightInUse;
		formatter.UnmanagedField(3, "FlashlightInUse", ref value3, state);
		formatter.UnmanagedField(4, "m_PreviousFormation", ref m_PreviousFormation, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartFlashlight>();
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
				Unsafe.AsRef(in m_FlashlightEnabled) = formatter.ReadPackable<CountableFlag>(state);
				break;
			case 1:
				m_Enabled = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				ForcedLookAtPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 3:
				FlashlightInUse = formatter.ReadUnmanaged<bool>(state);
				break;
			case 4:
				m_PreviousFormation = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
		OnAfterDeserialize();
	}
}
