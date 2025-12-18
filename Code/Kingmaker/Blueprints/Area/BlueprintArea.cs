using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.GameModes;
using Kingmaker.Localization;
using Kingmaker.Settings;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;
using UnityEngine.Serialization;
using Warhammer.Utility.Author;

namespace Kingmaker.Blueprints.Area;

[TypeId("6181425e72507c148a67d77624419aec")]
[OwlPackable(OwlPackableMode.NoGenerate)]
[ComponentName("Area/BlueprintArea")]
public class BlueprintArea : BlueprintAreaPart
{
	public enum SettingType
	{
		Unspecified,
		Hills,
		Plains,
		FirstWorld,
		Forest,
		Caves,
		Cyclops,
		LostLarnKeep
	}

	[SerializeField]
	[ShowCreator]
	private List<BlueprintAreaPartReference> m_Parts = new List<BlueprintAreaPartReference>();

	public float CameraScrollMultiplier = 1f;

	public CampingSettings CampingSettings;

	public RandomEncounterSettings RandomEncounterSettings;

	public Author AreaDesigner = Author.EugeneIvanov;

	public DesignersGame DesignerGame = DesignersGame.Unknown;

	public DesignersSound DesignerSound = DesignersSound.Unknown;

	[HideInInspector]
	public SettingType ArtSetting;

	public LocalizedString AreaName;

	public bool ExcludeFromSave;

	[Tooltip("Используется чтобы разрезать игру на части на консолях")]
	public ChunkId ChunkId;

	public List<LoadingScreenImage> LoadingScreenSprites;

	[SerializeField]
	[FormerlySerializedAs("DefaultPreset")]
	private BlueprintAreaPresetReference m_DefaultPreset;

	[SerializeField]
	private int m_CR;

	public bool OverrideCorruption;

	[ShowIf("OverrideCorruption")]
	public int CorruptionGrowth;

	[Tooltip("Areas, which scenes should be kept loaded when switching to this area")]
	[SerializeField]
	[FormerlySerializedAs("HotAreas")]
	private BlueprintAreaReference[] m_HotAreas;

	public virtual string AreaDisplayName
	{
		get
		{
			BlueprintAreaPart blueprintAreaPart = ((this == Game.Instance.CurrentlyLoadedArea && Parts.Contains((BlueprintAreaPartReference p) => p.Get() == Game.Instance.CurrentlyLoadedAreaPart)) ? Game.Instance.CurrentlyLoadedAreaPart : this);
			if (string.IsNullOrWhiteSpace(blueprintAreaPart?.LocalizedName))
			{
				return AreaName.Text;
			}
			return blueprintAreaPart.LocalizedName;
		}
	}

	public bool HasParts => m_Parts.HasItem((BlueprintAreaPartReference p) => p?.Get() != null);

	public BlueprintAreaPreset DefaultPreset
	{
		get
		{
			return m_DefaultPreset?.Get();
		}
		set
		{
			m_DefaultPreset = value.ToReference<BlueprintAreaPresetReference>();
		}
	}

	public ReferenceArrayProxy<BlueprintArea> HotAreas
	{
		get
		{
			BlueprintReference<BlueprintArea>[] hotAreas = m_HotAreas;
			return hotAreas;
		}
	}

	public virtual GameModeType AreaStatGameMode { get; } = GameModeType.Default;


	public virtual BlueprintCameraSettings CameraSettings => ConfigRoot.Instance.CameraRoot.GroundMapSettings;

	public bool IsNavmeshArea
	{
		get
		{
			if (AreaStatGameMode != GameModeType.StarSystem)
			{
				return AreaStatGameMode != GameModeType.GlobalMap;
			}
			return false;
		}
	}

	public bool IsGlobalmapArea => AreaStatGameMode == GameModeType.GlobalMap;

	public bool IsPartyArea => true;

	public bool NotPause
	{
		get
		{
			if (!(AreaStatGameMode == GameModeType.GlobalMap) && !(AreaStatGameMode == GameModeType.StarSystem))
			{
				return name == "VoidshipBridge";
			}
			return true;
		}
	}

	public IEnumerable<SceneReference> LightScenes => from part in PartsAndSelf
		select part.LightScene into sceneRef
		where sceneRef != null
		select sceneRef;

	public override IEnumerable<SceneReference> AudioScenes => base.AudioScenes.Concat(m_Parts.Where((BlueprintAreaPartReference p) => p?.Get() != null).SelectMany((BlueprintAreaPartReference p) => p.Get().AudioScenes));

	public List<BlueprintAreaPartReference> Parts => m_Parts;

	public IEnumerable<BlueprintAreaPart> PartsAndSelf => m_Parts.Where((BlueprintAreaPartReference p) => p?.Get() != null).Dereference().Append(this);

	public IEnumerable<SceneReference> AllScenesWithParts()
	{
		return PartsAndSelf.SelectMany((BlueprintAreaPart v) => v.GetAllScenes());
	}

	public IEnumerable<SceneReference> AllScenesWithParts(bool console)
	{
		return PartsAndSelf.SelectMany((BlueprintAreaPart v) => v.GetAllScenes(console));
	}

	[NotNull]
	public HashSet<string> GetHotSceneNames()
	{
		HashSet<string> hashSet = new HashSet<string>();
		if (!SettingsRoot.Game.Main.UseHotAreas)
		{
			return hashSet;
		}
		if (HotAreas.Length <= 0)
		{
			return hashSet;
		}
		hashSet.AddRange(from sr in AllScenesWithParts(console: false)
			select sr.SceneName);
		foreach (BlueprintArea hotArea in HotAreas)
		{
			hashSet.AddRange(from sr in hotArea.AllScenesWithParts(console: false)
				select sr.SceneName);
		}
		return hashSet;
	}

	public override IEnumerable<string> GetActiveSoundBankNames(bool isCurrentPart = false)
	{
		if (isCurrentPart)
		{
			return base.GetActiveSoundBankNames(isCurrentPart: true);
		}
		return base.GetActiveSoundBankNames().Concat(m_Parts.Where((BlueprintAreaPartReference p) => p?.Get() != null).SelectMany((BlueprintAreaPartReference p) => p.Get().GetActiveSoundBankNames()));
	}

	public override IEnumerable<SceneReference> GetStaticAndActiveDynamicScenes()
	{
		foreach (BlueprintAreaPart item in PartsAndSelf)
		{
			yield return item.StaticScene;
		}
		foreach (SceneReference activeDynamicScene in GetActiveDynamicScenes())
		{
			yield return activeDynamicScene;
		}
	}

	public IEnumerable<SceneReference> GetActiveDynamicScenes()
	{
		ISet<SceneReference> set = new HashSet<SceneReference>();
		set.Add(base.DynamicScene);
		foreach (BlueprintAreaPartReference part in m_Parts)
		{
			if (part?.Get() != null)
			{
				set.Add(part.Get().DynamicScene);
			}
		}
		foreach (BlueprintAreaMechanics activeAdditionalMechanic in Game.Instance.EtudesSystem.GetActiveAdditionalMechanics(this))
		{
			set.Add(activeAdditionalMechanic.Scene);
		}
		return set;
	}

	public int GetCR()
	{
		if (!BlueprintAreaHelper.OverridenCR.TryGetValue(AssetGuid, out var value))
		{
			return m_CR;
		}
		return value;
	}

	public IEnumerable<BlueprintAreaPart> GetParts()
	{
		return from p in m_Parts
			where p?.Get() != null
			select p.Get();
	}

	public IEnumerable<SceneReference> GetLightScenes()
	{
		return PartsAndSelf.Select((BlueprintAreaPart t) => t.GetLightScene());
	}

	public IEnumerable<SceneReference> GetAudioScenes(TimeOfDay timeOfDay)
	{
		return from part in PartsAndSelf
			select part.GetAudioScene(timeOfDay) into sceneRef
			where sceneRef?.IsDefined ?? false
			select sceneRef;
	}
}
