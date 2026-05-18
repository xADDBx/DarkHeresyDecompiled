using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Base;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintPortrait))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("02852e8357c14ddda3acc4f0a1f6f651")]
public class RaceAppearanceOverride : BlueprintComponent
{
	[Serializable]
	public class Entry
	{
		[SerializeField]
		[ValidateNotNull]
		private BlueprintRaceVisualPresetReference m_RacePreset;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Head;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Eyebrows;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Hair;

		[SerializeField]
		public EquipmentEntityLink Beard;

		[SerializeField]
		public EquipmentEntityLink Eyes;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Scar;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Tattoo;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Tattoo2;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Tattoo3;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Tattoo4;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Tattoo5;

		[SerializeField]
		[ValidateNotNull]
		[FormerlySerializedAs("Port")]
		public EquipmentEntityLink Augmentic1;

		[SerializeField]
		[ValidateNotNull]
		[FormerlySerializedAs("Port2")]
		public EquipmentEntityLink Augmentic2;

		public int HairRampIndex = -1;

		public int SkinRampIndex = -1;

		public int TattooRampIndex = -1;

		public int EyebrowsColorRampIndex = -1;

		public int BeardColorRampIndex = -1;

		public int EquipmentRampIndex = -1;

		public int EquipmentRampIndexSecondary = -1;

		public int EyesColorRampIndex = -1;

		public BlueprintRaceVisualPreset RacePreset => m_RacePreset;
	}

	public Gender Gender;

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("Race")]
	private BlueprintRaceReference m_Race;

	public Entry Default;

	public BlueprintRace Race => m_Race?.Get();
}
