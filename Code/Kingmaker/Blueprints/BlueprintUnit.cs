using System;
using System.Collections.Generic;
using System.Linq;
using Code.Framework.Editor.Blueprints.BlueprintUnitTemplates;
using Code.GameCore.Blueprints;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.Gameplay.Features.Encounter.Components;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Customization;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Components;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.UnitLogic.Visual.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.Unit.Utility;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Sound;
using Newtonsoft.Json;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[TypeId("fa4fa7e4548127a47a2846c91b051065")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintUnit : BlueprintUnitFact, IBlueprintCreateMechanicEntity<BaseUnitEntity>, IBlueprintUnitExportCharacter
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintUnit>
	{
	}

	[Serializable]
	public class UnitBody : IUnitBodyExtension
	{
		public UnitItemEquipmentHandSettings ItemEquipmentHandSettings = new UnitItemEquipmentHandSettings();

		[SerializeField]
		private BlueprintItemWeaponReference[] m_AdditionalLimbs = new BlueprintItemWeaponReference[0];

		[SerializeField]
		private BlueprintItemWeaponReference[] m_AdditionalSecondaryLimbs = new BlueprintItemWeaponReference[0];

		[SerializeField]
		private BlueprintItemArmorReference m_Armor;

		[SerializeField]
		private BlueprintItemEquipmentShirtReference m_Shirt;

		[SerializeField]
		private BlueprintItemEquipmentBeltReference m_Belt;

		[SerializeField]
		private BlueprintItemEquipmentHeadReference m_Head;

		[SerializeField]
		private BlueprintItemEquipmentGlassesReference m_Glasses;

		[SerializeField]
		private BlueprintItemEquipmentFeetReference m_Feet;

		[SerializeField]
		private BlueprintItemEquipmentGlovesReference m_Gloves;

		[SerializeField]
		private BlueprintItemEquipmentNeckReference m_Neck;

		[SerializeField]
		private BlueprintItemEquipmentRingReference m_Ring1;

		[SerializeField]
		private BlueprintItemEquipmentRingReference m_Ring2;

		[SerializeField]
		private BlueprintItemEquipmentWristReference m_Wrist;

		[SerializeField]
		private BlueprintItemEquipmentShouldersReference m_Shoulders;

		[SerializeField]
		private BlueprintItemEquipmentUsableReference[] m_QuickSlots = new BlueprintItemEquipmentUsableReference[4];

		[SerializeField]
		private BlueprintItemMechadendrite.BlueprintItemMechadendriteReference[] m_Mechadendrites = Array.Empty<BlueprintItemMechadendrite.BlueprintItemMechadendriteReference>();

		[JsonProperty]
		public UnitItemEquipmentHandSettings OverridenUnitItemEquipmentHandSettings { get; set; }

		public ReferenceArrayProxy<BlueprintItemWeapon> AdditionalLimbs
		{
			get
			{
				BlueprintReference<BlueprintItemWeapon>[] additionalLimbs = m_AdditionalLimbs;
				return additionalLimbs;
			}
		}

		public ReferenceArrayProxy<BlueprintItemWeapon> AdditionalSecondaryLimbs
		{
			get
			{
				BlueprintReference<BlueprintItemWeapon>[] additionalSecondaryLimbs = m_AdditionalSecondaryLimbs;
				return additionalSecondaryLimbs;
			}
		}

		public BlueprintItemArmor Armor => m_Armor?.Get();

		public BlueprintItemEquipmentShirt Shirt => m_Shirt?.Get();

		public BlueprintItemEquipmentBelt Belt => m_Belt?.Get();

		public BlueprintItemEquipmentHead Head => m_Head?.Get();

		public BlueprintItemEquipmentGlasses Glasses => m_Glasses?.Get();

		public BlueprintItemEquipmentFeet Feet => m_Feet?.Get();

		public BlueprintItemEquipmentGloves Gloves => m_Gloves?.Get();

		public BlueprintItemEquipmentNeck Neck => m_Neck?.Get();

		public BlueprintItemEquipmentRing Ring1 => m_Ring1?.Get();

		public BlueprintItemEquipmentRing Ring2 => m_Ring2?.Get();

		public BlueprintItemEquipmentWrist Wrist => m_Wrist?.Get();

		public BlueprintItemEquipmentShoulders Shoulders => m_Shoulders?.Get();

		public ReferenceArrayProxy<BlueprintItemEquipmentUsable> QuickSlots
		{
			get
			{
				BlueprintReference<BlueprintItemEquipmentUsable>[] quickSlots = m_QuickSlots;
				return quickSlots;
			}
		}

		public ReferenceArrayProxy<BlueprintItemMechadendrite> Mechadendrites
		{
			get
			{
				BlueprintReference<BlueprintItemMechadendrite>[] mechadendrites = m_Mechadendrites;
				return mechadendrites;
			}
		}

		[CanBeNull]
		public BlueprintItemEquipmentHand GetHandEquipment(int i, bool main, UnitItemEquipmentHandSettings settings)
		{
			return i switch
			{
				0 => main ? settings.PrimaryHand : settings.SecondaryHand, 
				1 => main ? settings.PrimaryHandAlternative1 : settings.SecondaryHandAlternative1, 
				_ => null, 
			};
		}

		void IUnitBodyExtension.SetBody(PartUnitBody body, BlueprintUnit blueprintUnit)
		{
		}
	}

	private interface IUnitBodyExtension
	{
		void SetBody(PartUnitBody body, BlueprintUnit blueprintUnit);
	}

	[SerializeField]
	private BlueprintArmyTypeReference m_Army;

	public new SharedStringAsset LocalizedName;

	public VoIdField VoId = new VoIdField();

	public bool HasAdditionalVoIds;

	[ShowIf("HasAdditionalVoIds")]
	public List<VoIdField> AdditionalVoIds = new List<VoIdField>();

	public Gender Gender;

	public Size Size = Size.Medium;

	public Color Color = new Color(0.15f, 0.15f, 0.15f, 1f);

	[SerializeField]
	private BlueprintRaceReference m_Race;

	[SerializeField]
	private BlueprintPortraitReference m_Portrait;

	[ValidateNotNull]
	public UnitViewLink Prefab;

	[SerializeField]
	[CanBeNull]
	private UnitCustomizationPresetReference m_CustomizationPreset;

	[SerializeField]
	private BlueprintUnitVisualSettings.Reference m_VisualSettings;

	[SerializeField]
	private BlueprintUnitAsksListReference m_Asks;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintFactionReference m_Faction;

	public FactionOverrides FactionOverrides;

	[SerializeField]
	private BlueprintItemReference[] m_StartingInventory;

	public UnitSubtype Subtype = UnitSubtype.Default;

	public BlueprintUnitTemplateReference Template = new BlueprintUnitTemplateReference();

	public List<BlueprintAbilityReference> AbilitiesFromTemplate = new List<BlueprintAbilityReference>();

	public List<BlueprintUnitFactReference> FeaturesFromTemplate = new List<BlueprintUnitFactReference>();

	[Tooltip("If true, unit won't return to target position on combat leave")]
	public bool IsStayOnSameSpotAfterCombat;

	[Header("Body")]
	public UnitBody Body = new UnitBody();

	public bool OverrideBodyParts;

	[SerializeField]
	[ShowIf("OverrideBodyParts")]
	private BpRef<BlueprintBodyPart>[] _bodyParts;

	[Header("Stats")]
	public UnitStatModifiers StatModifiers = new UnitStatModifiers();

	public float WarhammerMovementApPerCell = 1f;

	public float WarhammerMovementApPerCellThreateningArea = 3f;

	public int WarhammerInitialAPBlue = 3;

	public int WarhammerInitialAPYellow = 3;

	public Feet Speed = 30.Feet();

	public int Defence;

	public int ArmorDamageReduction;

	[Header("Facts")]
	[SerializeField]
	private BlueprintUnitFactReference[] m_AddFacts;

	[SerializeField]
	public List<InitialAlignmentShift> InitialAlignmentShifts = new List<InitialAlignmentShift>();

	[Tooltip("Trap actors, mapobject cast targets and other units that are not actually subject ot game mechanics. Cheaters can use any ability, are never ingame but do show FX")]
	public bool IsCheater;

	public UnitDifficultyType DifficultyType;

	private AddTags m_CachedTags;

	private int? m_DefaultLevel;

	[CanBeNull]
	public BlueprintArmyType Army
	{
		get
		{
			return m_Army;
		}
		set
		{
			m_Army = value.ToReference<BlueprintArmyTypeReference>();
		}
	}

	public BpRef<BlueprintBodyPart>[] BodyParts
	{
		get
		{
			if (!OverrideBodyParts)
			{
				return ConfigRoot.Instance.SystemMechanics.DefaultHumanoidBodyParts;
			}
			return _bodyParts;
		}
	}

	public string CharacterName
	{
		get
		{
			if ((bool)LocalizedName)
			{
				return LocalizedName.String;
			}
			return "-unit name not set-";
		}
	}

	public BlueprintRace Race => m_Race?.Get();

	private bool IsNewStat => Army != null;

	private bool IsOldStat => !IsNewStat;

	[NotNull]
	public BlueprintPortrait PortraitSafe
	{
		get
		{
			if (m_Portrait.IsEmpty())
			{
				if (Gender != 0)
				{
					return ConfigRoot.Instance.UIConfig.Portraits.FemalePlaceholderPortrait;
				}
				return ConfigRoot.Instance.UIConfig.Portraits.MalePlaceholderPortrait;
			}
			return m_Portrait.Get();
		}
	}

	public UnitCustomizationPreset CustomizationPreset
	{
		get
		{
			return m_CustomizationPreset?.Get();
		}
		set
		{
			m_CustomizationPreset = value.ToReference<UnitCustomizationPresetReference>();
		}
	}

	public UnitVisualSettings VisualSettings => m_VisualSettings?.Get()?.Settings ?? UnitVisualSettings.Empty;

	public BlueprintUnitAsksList Asks => m_Asks?.Get();

	public BlueprintFaction Faction => m_Faction?.Get();

	public ReferenceArrayProxy<BlueprintItem> StartingInventory
	{
		get
		{
			BlueprintReference<BlueprintItem>[] startingInventory = m_StartingInventory;
			return startingInventory;
		}
	}

	public ReferenceArrayProxy<BlueprintUnitFact> AddFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] addFacts = m_AddFacts;
			return addFacts;
		}
	}

	public int? OverrideCR => this.GetComponent<OverrideUnitCRComponent>()?.OverrideCR;

	[CanBeNull]
	public AddTags Tags => BlueprintComponentExtendAsObject.Or(m_CachedTags, null) ?? BlueprintComponentExtendAsObject.Or(m_CachedTags = this.GetComponent<AddTags>(), null);

	public IEnumerable<BlueprintFaction> AttackFactions
	{
		get
		{
			HashSet<BlueprintFaction> hashSet = new HashSet<BlueprintFaction>(Faction.AttackFactions);
			hashSet.UnionWith(FactionOverrides.AttackFactionsToAdd);
			hashSet.ExceptWith(FactionOverrides.AttackFactionsToRemove);
			return hashSet;
		}
	}

	public bool IsCompanion => Faction == ConfigRoot.Instance.SystemMechanics.PlayerFaction;

	public bool UseArmorOfEquipment
	{
		get
		{
			if (!StatModifiers.UseArmorOfEquipment)
			{
				return IsCompanion;
			}
			return true;
		}
	}

	protected override bool ShowDisplayName => false;

	protected override bool ShowDescription => false;

	public BlueprintUnitFact[] GetFactsFromTemplate()
	{
		List<BlueprintUnitFact> list = new List<BlueprintUnitFact>();
		BlueprintUnitTemplate blueprintUnitTemplate = Template?.Get();
		if (blueprintUnitTemplate != null)
		{
			list.AddRange(blueprintUnitTemplate.MandatoryFacts.Select((BlueprintUnitFactReference i) => i.Get()));
			foreach (BlueprintAbilityReference item in AbilitiesFromTemplate)
			{
				if (blueprintUnitTemplate.AvailableAbilities.Contains(item))
				{
					list.Add(item.Get());
				}
			}
			foreach (BlueprintUnitFactReference item2 in FeaturesFromTemplate)
			{
				if (blueprintUnitTemplate.AvailableFeatures.Contains(item2))
				{
					list.Add(item2.Get());
				}
			}
		}
		return list.ToArray();
	}

	protected override Type GetFactType()
	{
		return typeof(EntityFact);
	}

	public bool CheckEqualsWithPrototype(BlueprintUnit other)
	{
		if (this != other)
		{
			if (base.PrototypeLink is BlueprintUnit blueprintUnit)
			{
				return blueprintUnit.CheckEqualsWithPrototype(other);
			}
			return false;
		}
		return true;
	}

	public void PreloadResources()
	{
		VisualSettings.ArmorFx.Preload();
		VisualSettings.BloodPuddleFx.Preload();
		VisualSettings.DismemberFx.Preload();
		VisualSettings.RipLimbsApartFx.Preload();
		ConfigRoot.Instance.HitSystemRoot.HitEffects.FirstOrDefault((HitEntry b) => b.Type == VisualSettings.SurfaceType)?.PreloadResources();
	}

	public BaseUnitEntity CreateEntity(string uniqueId = null, bool isInGame = true)
	{
		if (uniqueId == null)
		{
			uniqueId = Uuid.Instance.CreateString();
		}
		return Entity.Initialize(new UnitEntity(uniqueId, isInGame, this));
	}

	public int GetDefaultLevel()
	{
		if (m_DefaultLevel.HasValue)
		{
			return m_DefaultLevel.Value;
		}
		m_DefaultLevel = 0;
		BlueprintUnitFactReference[] addFacts = m_AddFacts;
		for (int i = 0; i < addFacts.Length; i++)
		{
			foreach (ApplyCareerPath component in addFacts[i].Get().GetComponents<ApplyCareerPath>())
			{
				if (component.CareerPath is BlueprintCareerPath)
				{
					m_DefaultLevel += component.Ranks;
				}
			}
		}
		m_DefaultLevel = Mathf.Max(1, m_DefaultLevel.Value);
		return m_DefaultLevel.Value;
	}

	public void TrySetupOverridenUnitBodyHandsSettings()
	{
		OverrideUnitBodyWithRandomHandsSettings component = this.GetComponent<OverrideUnitBodyWithRandomHandsSettings>();
		if (component == null)
		{
			return;
		}
		float num = PFStatefulRandom.Blueprints.Range(0f, component.TotalWeightPercent);
		int num2 = 0;
		UnitItemEquipmentHandSettings unitItemEquipmentHandSettings = null;
		UnitItemEquipmentHandSettingsWithWeights[] array = component.SettingsWithWeights.EmptyIfNull();
		foreach (UnitItemEquipmentHandSettingsWithWeights unitItemEquipmentHandSettingsWithWeights in array)
		{
			num2 += unitItemEquipmentHandSettingsWithWeights.Weight;
			if (!((float)num2 <= num))
			{
				unitItemEquipmentHandSettings = unitItemEquipmentHandSettingsWithWeights.UnitHandsSettings;
				break;
			}
		}
		if (unitItemEquipmentHandSettings != null)
		{
			Body.OverridenUnitItemEquipmentHandSettings = unitItemEquipmentHandSettings;
		}
	}

	void IBlueprintUnitExportCharacter.SyncFacts(BlueprintUnitFact[] facts)
	{
	}

	void IBlueprintUnitExportCharacter.SyncBody(PartUnitBody body)
	{
		((IUnitBodyExtension)Body)?.SetBody(body, this);
	}
}
