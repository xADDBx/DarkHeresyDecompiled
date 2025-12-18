using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
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

	[Header("New Game CharGen Paths")]
	[SerializeField]
	private BlueprintOriginPath.Reference m_NewGameCustomChargenPath;

	[SerializeField]
	private BlueprintOriginPath.Reference m_NewGamePregenChargenPath;

	[Header("New Companion CharGen Paths")]
	[SerializeField]
	private BlueprintOriginPath.Reference m_NewCompanionCustomChargenPath;

	[SerializeField]
	private BlueprintOriginPath.Reference m_NewCompanionPregenChargenPath;

	[Header("New Companion Navigator CharGen Paths")]
	[SerializeField]
	private BlueprintOriginPath.Reference m_NewCompanionNavigatorCustomChargenPath;

	[SerializeField]
	private BlueprintOriginPath.Reference m_NewCompanionNavigatorPregenChargenPath;

	[NotNull]
	[SerializeField]
	private BlueprintPortraitReference[] m_Portraits;

	[NotNull]
	[SerializeField]
	private BlueprintPortraitReference m_CustomPortrait;

	[NotNull]
	[SerializeField]
	private BlueprintPortraitReference m_PlaceholderPortrait;

	[NotNull]
	public string PortraitsFormat = ".png";

	[NotNull]
	public string PortraitSmallName = "Small";

	[NotNull]
	public string PortraitMediumName = "Medium";

	[NotNull]
	public string PortraitBigName = "Fulllength";

	[NotNull]
	public string PortraitFolderName = "Portraits";

	[NotNull]
	public SpriteLink BasePortraitSmall;

	[NotNull]
	public SpriteLink BasePortraitMedium;

	[NotNull]
	public SpriteLink BasePortraitBig;

	[NotNull]
	[SerializeField]
	private BlueprintUnitAsksListReference[] m_Voices;

	[SerializeField]
	private int m_MaleVoiceDefaultId;

	[SerializeField]
	private int m_FemaleVoiceDefaultId;

	[Header("New Game Pregens")]
	[SerializeField]
	private BlueprintUnitReference[] m_Pregens;

	[Header("New Companion Pregens")]
	[SerializeField]
	private PregenEntry[] CompanionPregens;

	private List<ChargenUnit> m_PregensForChargen;

	private List<ChargenUnit> m_CompanionPregensForChargen;

	private List<ChargenUnit> m_ShipsForChargen;

	private List<ChargenUnit> m_AllUnits;

	private bool m_ClearUnitsAfterUse = true;

	[NotNull]
	[AssetPicker("")]
	public Character MaleDoll;

	[NotNull]
	[AssetPicker("")]
	public Character FemaleDoll;

	public AnimSnapToClothAnimationSettings TailAnimationSettings;

	public EquipmentEntityLink[] MaleClothes;

	public EquipmentEntityLink[] FemaleClothes;

	public List<EquipmentEntityLink> MaleDontUnequip;

	public List<EquipmentEntityLink> FemaleDontUnequip;

	public EquipmentEntityLink[] WarpaintsForCustomization;

	[NotNull]
	public List<BlueprintCompanionStoryReference> CustomCompanionStories;

	public List<BlueprintUnitReference> CustomCompanions;

	public PregenCharacterNames PregenCharacterNames;

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

	public int MaleVoiceDefaultId => m_MaleVoiceDefaultId;

	public ReferenceArrayProxy<BlueprintUnit> Pregens
	{
		get
		{
			BlueprintReference<BlueprintUnit>[] pregens = m_Pregens;
			return pregens;
		}
	}

	public int FemaleVoiceDefaultId => m_FemaleVoiceDefaultId;

	public void EnsureNewGamePregens(Action<List<ChargenUnit>> readyCallback)
	{
		EnsureChargenUnits(ref m_PregensForChargen, PrepareNewGamePregensCoroutine(readyCallback), readyCallback);
	}

	public void EnsureCompanionPregens(Action<List<ChargenUnit>> readyCallback, CharGenCompanionType companionType)
	{
		EnsureChargenUnits(ref m_CompanionPregensForChargen, PrepareCompanionPregensCoroutine(readyCallback, companionType), readyCallback);
	}

	public void EnsureShipPregens(Action<List<ChargenUnit>> readyCallback)
	{
		EnsureChargenUnits(ref m_ShipsForChargen, PrepareShipPregensCoroutine(readyCallback), readyCallback);
	}

	private void EnsureChargenUnits(ref List<ChargenUnit> unitsList, IEnumerator prepareCoroutine, Action<List<ChargenUnit>> readyCallback)
	{
		if (unitsList == null)
		{
			MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(prepareCoroutine);
		}
		else
		{
			readyCallback(unitsList);
		}
	}

	public bool IsBlueprintCompanionPregen(BlueprintUnit unitBlueprint)
	{
		if (unitBlueprint != null)
		{
			return CompanionPregens.Any((PregenEntry p) => p.UnitBlueprint.Is(unitBlueprint));
		}
		return false;
	}

	private IEnumerator PrepareNewGamePregensCoroutine(Action<List<ChargenUnit>> readyCallback = null)
	{
		m_PregensForChargen = new List<ChargenUnit>();
		yield return PrepareChargenUnitsCoroutine(Pregens, m_PregensForChargen, readyCallback);
	}

	private IEnumerator PrepareCompanionPregensCoroutine(Action<List<ChargenUnit>> readyCallback = null, CharGenCompanionType companionType = CharGenCompanionType.Common)
	{
		m_CompanionPregensForChargen = new List<ChargenUnit>();
		IEnumerable<BlueprintUnit> units = from i in CompanionPregens
			where i.CompanionType == companionType
			select i.UnitBlueprint.Get();
		yield return PrepareChargenUnitsCoroutine(units, m_CompanionPregensForChargen, readyCallback);
	}

	private IEnumerator PrepareShipPregensCoroutine(Action<List<ChargenUnit>> readyCallback = null)
	{
		m_ShipsForChargen = new List<ChargenUnit>();
		yield break;
	}

	private IEnumerator PrepareChargenUnitsCoroutine(IEnumerable<BlueprintUnit> units, List<ChargenUnit> resultList, Action<List<ChargenUnit>> readyCallback = null)
	{
		if (m_AllUnits == null)
		{
			m_AllUnits = new List<ChargenUnit>();
		}
		foreach (BlueprintUnit unit in units)
		{
			ChargenUnit item = new ChargenUnit(unit);
			resultList.Add(item);
			m_AllUnits.Add(item);
			yield return null;
		}
		readyCallback?.Invoke(resultList);
	}

	public void DisposeUnitsForChargen()
	{
		foreach (ChargenUnit item in m_AllUnits.EmptyIfNull())
		{
			if (ShouldDisposeChargenUnit(item.Unit))
			{
				item.Unit.Dispose();
			}
			if (!m_ClearUnitsAfterUse)
			{
				item.Unit = null;
				item.RecreateUnit();
			}
		}
		if (m_ClearUnitsAfterUse)
		{
			m_AllUnits?.Clear();
			m_AllUnits = null;
			m_PregensForChargen?.Clear();
			m_PregensForChargen = null;
			m_CompanionPregensForChargen?.Clear();
			m_CompanionPregensForChargen = null;
			m_ShipsForChargen?.Clear();
			m_ShipsForChargen = null;
		}
	}

	private bool ShouldDisposeChargenUnit(BaseUnitEntity unit)
	{
		if (unit.IsDisposed || unit.IsMainCharacter)
		{
			return false;
		}
		UnitPartCompanion companionOptional = unit.GetCompanionOptional();
		bool flag;
		if (companionOptional != null)
		{
			CompanionState state = companionOptional.State;
			if ((uint)(state - 1) <= 1u || state == CompanionState.InPartyDetached)
			{
				flag = true;
				goto IL_0033;
			}
		}
		flag = false;
		goto IL_0033;
		IL_0033:
		if (flag)
		{
			return false;
		}
		return true;
	}
}
