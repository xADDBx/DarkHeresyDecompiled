using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.UIConfigComponents;
using Kingmaker.UI.Sound;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("352f2c3d37d66f64eaf3a156026c8882")]
public class UIConfig : BlueprintScriptableObject
{
	[SerializeField]
	private UIViewConfigs.Reference m_ViewConfigs;

	[SerializeField]
	private BlueprintUISoundReference m_BlueprintUISound;

	[SerializeField]
	private BlueprintUINetLobbyTutorial.Reference m_BlueprintUINetLobbyTutorial;

	[SerializeField]
	private BlueprintUILocalMapLegend.Reference m_BlueprintUILocalMapLegend;

	[SerializeField]
	private BpRef<ViewHighlightingColors> m_ViewHighlightingColors;

	[SerializeField]
	private BpRef<BlueprintFeedbackConfig> m_FeedbackConfig;

	public Color PaperSaberColor = Color.red;

	public Sprite KeyArt;

	public Sprite DlcEntityKeyArt;

	public VideoLink KeyVideoMainMenu;

	public GameObject DebugBubble;

	public LogColors LogColors;

	public DialogCueColors m_DefaultDialogCueColors;

	public GlossaryColors PaperGlossaryColors;

	public GlossaryColors DigitalGlossaryColors;

	public TooltipColors TooltipColors;

	public TutorialColors TutorialColors;

	public ModifierColors ModifierColors;

	public DigitalColors DigitalColors;

	public UIIcons UIIcons;

	public CombatTextColors CombatTextColors;

	public PreciseAttackColors PreciseAttackColors;

	public UnitInfoColors UnitInfoColors;

	public PowerBalanceColors PowerBalanceColors;

	public Color StatPositiveColor;

	public Color StatNegativeColor;

	public Color LinkColor;

	public Sprite TransparentImage;

	public Sprite DefaultNetAvatar;

	[Header("Coop Colors")]
	public List<Color> CoopPlayersPingsColors = new List<Color>();

	public List<Material> CoopPlayersPingsMaterials = new List<Material>();

	[Header("Items Description")]
	public LocalizedString ItemOriginOwnerDescription;

	public LocalizedString ItemVendorDescription;

	public LocalizedString EmptyString;

	public TextFormats TextFormats = new TextFormats();

	public int SubTextPercentSize = 70;

	[Space]
	[Header("RandomColors")]
	public Color32[] RandomColors = new Color32[10];

	public EquipSlotTypeIcons TypeIcons;

	[SerializeField]
	private BpRef<EnumWeaponStatIcons> m_WeaponStatIcons;

	public UnitPortraits Portraits;

	public QuestNotificationStateColor QuestStateColor;

	public ChapterList ChapterList;

	public BlueprintEncyclopediaChapterReference EncyclopediaDefaultPage;

	public BlueprintEncyclopediaChapterReference BookEventsChapter;

	public BlueprintEncyclopediaChapterReference AstropathBriefsChapter;

	public int DefaultConsoleHintScaleInText = 150;

	public const float OvertipDistanceReveal = 6.35f;

	public TMP_FontAsset DefaultTMPFontAsset;

	public TMP_SpriteAsset DefaultTMPSriteAsset;

	public CreditsGroups Credits;

	public AcronymsConfig AcronymsConfig;

	public FeatureFiltersIcons FiltersIcons;

	[Header("Talent Groups")]
	public Color SingleAcronymColor;

	public Color GroupAcronymColor;

	public TalentGroups TalentGroups = new TalentGroups();

	public static UIConfig Instance => ConfigRoot.Instance.UIConfig;

	[field: SerializeField]
	public TextStyle DefaultTextStyle { get; private set; }

	public EnumWeaponStatIcons WeaponStatIcons => m_WeaponStatIcons;

	[field: SerializeField]
	public CombatConfig CombatConfig { get; private set; }

	[field: SerializeField]
	public FeatureTagsConfig FeatureTagsConfig { get; private set; }

	[Header("Detective")]
	[field: SerializeField]
	public DetectiveConfig DetectiveConfig { get; private set; }

	public BlueprintUISound BlueprintUISound => m_BlueprintUISound?.Get();

	public BlueprintUINetLobbyTutorial BlueprintUINetLobbyTutorial => m_BlueprintUINetLobbyTutorial?.Get();

	public BlueprintUILocalMapLegend BlueprintUILocalMapLegend => m_BlueprintUILocalMapLegend?.Get();

	public UIViewConfigs ViewConfigs => m_ViewConfigs?.Get();

	public ViewHighlightingColors ViewHighlightingColors => m_ViewHighlightingColors?.Blueprint;

	public BlueprintFeedbackConfig FeedbackConfig => m_FeedbackConfig?.Blueprint;
}
