using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Base;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[ComponentName("Root/BlueprintCharGenRoot")]
[TypeId("0c3318b6d4f6dab45ad0f0d4d73fd0f6")]
public class BlueprintCharGenRoot : BlueprintScriptableObject
{
	[Serializable]
	public class PregenEntry
	{
		public CharGenCompanionType CompanionType;

		public BlueprintUnitReference UnitBlueprint;
	}

	[Serializable]
	public class VoiceEntry
	{
		public VoIdField VoId;

		public MusicStateHandler.MusicChargenPCVoice MusicChargenVoice;
	}

	[Serializable]
	public class GenderDollConfig
	{
		[NotNull]
		[AssetPicker("")]
		[Tooltip("Character prefab used as the doll base in CharGen and inventory screens. Must have a UnitEntityView component.")]
		public Character Doll;

		[Tooltip("Equipment entities always added on top of the doll (underwear / base clothes layer). Used by CharGenDollRoom and DollState.")]
		public EquipmentEntityLink[] Clothes;

		[Tooltip("Equipment entities that are NOT removed when the unclothing cutscene runs (UnclotheRT command). Typically underwear items.")]
		public List<EquipmentEntityLink> DontUnequip;
	}

	[Header("New Game CharGen Paths")]
	[Tooltip("Origin path used when the player creates a fully custom character at New Game start.")]
	[SerializeField]
	private BlueprintOriginPath.Reference m_NewGameCustomChargenPath;

	[Tooltip("Origin path used when the player picks a pregen character at New Game start.")]
	[SerializeField]
	private BlueprintOriginPath.Reference m_NewGamePregenChargenPath;

	[Header("New Companion CharGen Paths")]
	[Tooltip("Origin path used when creating a fully custom companion.")]
	[SerializeField]
	private BlueprintOriginPath.Reference m_NewCompanionCustomChargenPath;

	[Tooltip("Origin path used when picking a pregen companion.")]
	[SerializeField]
	private BlueprintOriginPath.Reference m_NewCompanionPregenChargenPath;

	[Header("New Companion Navigator CharGen Paths")]
	[Tooltip("Origin path used when creating a fully custom Navigator companion.")]
	[SerializeField]
	private BlueprintOriginPath.Reference m_NewCompanionNavigatorCustomChargenPath;

	[Tooltip("Origin path used when picking a pregen Navigator companion.")]
	[SerializeField]
	private BlueprintOriginPath.Reference m_NewCompanionNavigatorPregenChargenPath;

	[Header("Portraits")]
	[Tooltip("All built-in portraits available for selection in CharGen.")]
	[NotNull]
	[SerializeField]
	private BlueprintPortraitReference[] m_Portraits;

	[Tooltip("Placeholder portrait shown when the player has chosen a custom (external) portrait but it hasn't loaded yet.")]
	[NotNull]
	[SerializeField]
	private BlueprintPortraitReference m_CustomPortrait;

	[Tooltip("Fallback portrait shown when no portrait is assigned (e.g. companions before portrait selection).")]
	[NotNull]
	[SerializeField]
	private BlueprintPortraitReference m_PlaceholderPortrait;

	[Header("Portrait File Naming")]
	[Tooltip("File extension for portrait images used when reading custom portrait folders.")]
	[NotNull]
	public string PortraitsFormat = ".png";

	[Tooltip("File name (without extension) of the small portrait variant inside a custom portrait folder.")]
	[NotNull]
	public string PortraitSmallName = "Small";

	[Tooltip("File name (without extension) of the medium portrait variant inside a custom portrait folder.")]
	[NotNull]
	public string PortraitMediumName = "Medium";

	[Tooltip("File name (without extension) of the full-length portrait variant inside a custom portrait folder.")]
	[NotNull]
	public string PortraitBigName = "Fulllength";

	[Tooltip("Name of the root folder where custom portrait subfolders are stored.")]
	[NotNull]
	public string PortraitFolderName = "Portraits";

	[Header("Default Portrait Sprites")]
	[Tooltip("Default small portrait sprite shown as a fallback when a portrait image is missing.")]
	[NotNull]
	public SpriteLink BasePortraitSmall;

	[Tooltip("Default medium portrait sprite shown as a fallback when a portrait image is missing.")]
	[NotNull]
	public SpriteLink BasePortraitMedium;

	[Tooltip("Default full-length portrait sprite shown as a fallback when a portrait image is missing.")]
	[NotNull]
	public SpriteLink BasePortraitBig;

	[Header("Voices")]
	[Tooltip("All voice sets available for selection in CharGen.")]
	[NotNull]
	[SerializeField]
	private BlueprintUnitAsksListReference[] m_Voices;

	[Header("Voices")]
	[Tooltip("All voice sets available for selection in CharGen.")]
	[NotNull]
	[SerializeField]
	private VoiceEntry[] m_VoiceEntries;

	[Tooltip("Index into Voices array pre-selected by default for male characters in CharGen.")]
	[SerializeField]
	private int m_MaleVoiceDefaultId;

	[Tooltip("Index into Voices array pre-selected by default for female characters in CharGen.")]
	[SerializeField]
	private int m_FemaleVoiceDefaultId;

	[Header("New Game Pregens")]
	[Tooltip("Pregen unit blueprints shown in the New Game CharGen pregen selection screen. Loaded at runtime by MainMenuChargenUnits.")]
	[SerializeField]
	private BlueprintUnitReference[] m_Pregens;

	[Header("New Companion Pregens")]
	[Tooltip("Pregen companion entries (with companion slot type) available in the New Companion CharGen screen. Used by IsBlueprintCompanionPregen.")]
	[SerializeField]
	private PregenEntry[] CompanionPregens;

	[Header("Doll Visuals")]
	[Tooltip("Doll configuration for male characters: base Character prefab, base clothes layer, and unclothing exceptions. Used by CharGenDollRoom, DollData, DollState, UnclotheRT.")]
	public GenderDollConfig MaleDollConfig;

	[Tooltip("Doll configuration for female characters: base Character prefab, base clothes layer, and unclothing exceptions. Used by CharGenDollRoom, DollData, DollState, UnclotheRT.")]
	public GenderDollConfig FemaleDollConfig;

	[Tooltip("Shader applied to equipment entities to support dye/colorization in CharGen and the inventory doll room.")]
	public Shader EquipmentColorizerShader;

	[Tooltip("Prefab carrying one or more DissolveSetup components. Instantiated on the active doll by CharacterDollRoom on Show and after each SetupUnit; the prefab's DissolveSettings are pushed into the doll's StandardMaterialController.DissolveController to play a one-shot dissolve-in appear animation. The prefab is responsible for destroying itself once the animation has played out — the doll room does not schedule a destroy. When the animation reaches its Lifetime, DissolveAnimationController prunes it from its Animations list and reverts the materials from snapshots automatically.")]
	public GameObject CharacterAppearEffectPrefab;

	[Tooltip("Settings controlling how the tail bone snaps to cloth animation during doll room idle. Used by Character tail animation system.")]
	public AnimSnapToClothAnimationSettings TailAnimationSettings;

	[Header("Customization")]
	[Tooltip("Companion story blueprints available when creating a custom companion. Shown as backstory options in companion CharGen.")]
	[NotNull]
	public List<BlueprintCompanionStoryReference> CustomCompanionStories;

	[Tooltip("Unit blueprints for fully custom companions the player can recruit. Listed in the New Companion selection screen.")]
	public List<BlueprintUnitReference> CustomCompanions;

	[Header("Misc")]
	[Tooltip("Name tables used to display default names for pregen characters in the CharGen UI.")]
	public PregenCharacterNames PregenCharacterNames;

	[Tooltip("Equipment entity reference for the flashlight item. Used to equip/unequip the flashlight at runtime.")]
	public KingmakerEquipmentEntityReference Flashlight;

	public static BlueprintCharGenRoot Instance => ConfigRoot.Instance.CharGenRoot;

	public BlueprintOriginPath NewGameCustomChargenPath => m_NewGameCustomChargenPath?.Get();

	public BlueprintOriginPath NewGamePregenChargenPath => m_NewGamePregenChargenPath?.Get();

	public BlueprintOriginPath NewCompanionCustomChargenPath => m_NewCompanionCustomChargenPath?.Get();

	public BlueprintOriginPath NewCompanionPregenChargenPath => m_NewCompanionPregenChargenPath?.Get();

	public BlueprintOriginPath NewCompanionNavigatorCustomChargenPath => m_NewCompanionNavigatorCustomChargenPath?.Get();

	public BlueprintOriginPath NewCompanionNavigatorPregenChargenPath => m_NewCompanionNavigatorPregenChargenPath?.Get();

	public ReferenceArrayProxy<BlueprintPortrait> Portraits
	{
		get
		{
			BlueprintReference<BlueprintPortrait>[] portraits = m_Portraits;
			return portraits;
		}
	}

	public BlueprintPortrait CustomPortrait => m_CustomPortrait.Get();

	public ReferenceArrayProxy<BlueprintUnitAsksList> Voices
	{
		get
		{
			BlueprintReference<BlueprintUnitAsksList>[] voices = m_Voices;
			return voices;
		}
	}

	public VoiceEntry[] VoiceEntries => m_VoiceEntries;

	public int MaleVoiceDefaultId => m_MaleVoiceDefaultId;

	public int FemaleVoiceDefaultId => m_FemaleVoiceDefaultId;

	public ReferenceArrayProxy<BlueprintUnit> Pregens
	{
		get
		{
			BlueprintReference<BlueprintUnit>[] pregens = m_Pregens;
			return pregens;
		}
	}

	public GenderDollConfig GetDollConfig(Gender gender)
	{
		if (gender != 0)
		{
			return FemaleDollConfig;
		}
		return MaleDollConfig;
	}

	public bool IsBlueprintCompanionPregen(BlueprintUnit unitBlueprint)
	{
		if (unitBlueprint != null)
		{
			return CompanionPregens.Any((PregenEntry p) => p.UnitBlueprint.Is(unitBlueprint));
		}
		return false;
	}
}
