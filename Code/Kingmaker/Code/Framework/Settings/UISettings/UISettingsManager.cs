using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;

namespace Kingmaker.Code.Framework.Settings.UISettings;

public class UISettingsManager
{
	public enum SettingsScreen
	{
		Game,
		Difficulty,
		Sound,
		Graphics,
		Controls,
		StartGameDifficulty,
		Display,
		Accessiability,
		Language,
		SafeZone,
		Development
	}

	private bool m_Initialized;

	private bool m_KeyBindingsCached;

	private readonly Dictionary<string, UISettingsEntityKeyBinding> m_KeyBindCache = new Dictionary<string, UISettingsEntityKeyBinding>();

	public bool IsNewKeyBindingSelectionHappening;

	private readonly List<UISettingsGroup> m_GameSettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_SoundSettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_SafeZoneSettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_DisplaySettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_AccessiablitySettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_LanguageFirstLaunchSettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_GraphicsSettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_DifficultySettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_ControlSettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_StartGameDifficultySettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_DevelopmentSettingsList = new List<UISettingsGroup>();

	private static UISettingsRoot UISettingsRoot => ConfigRoot.Instance.UISettingsRoot;

	public IEnumerable<UISettingsEntityKeyBinding> KeyBindings => (from k in UISettingsRoot.Controls.SelectMany((UISettingsGroup m) => m.VisibleSettingsList)
		where k != null && k.Type == SettingsListItemType.Keybind
		select k).OfType<UISettingsEntityKeyBinding>();

	public UISettingsEntityKeyBinding GetBindByName(string name)
	{
		if (!m_KeyBindingsCached)
		{
			BuildKeyBindCache();
		}
		m_KeyBindCache.TryGetValue(name, out var value);
		return value;
	}

	private void BuildKeyBindCache()
	{
		if (m_KeyBindingsCached)
		{
			return;
		}
		foreach (UISettingsEntityKeyBinding keyBinding in KeyBindings)
		{
			m_KeyBindCache.Add(keyBinding.name, keyBinding);
		}
		m_KeyBindingsCached = true;
	}

	public void Initialize()
	{
		if (!m_Initialized)
		{
			UISettingsRoot.Instance.LinkToSettings();
			UISettingsRoot.Instance.InitializeSettings();
			m_Initialized = true;
			InitializeSettingsList(m_GameSettingsList, UISettingsRoot.GameSettings);
			InitializeSettingsList(m_DifficultySettingsList, UISettingsRoot.DifficultySettings);
			InitializeSettingsList(m_SoundSettingsList, UISettingsRoot.SoundSettings);
			InitializeSettingsList(m_SafeZoneSettingsList, UISettingsRoot.SafeZone);
			InitializeSettingsList(m_DisplaySettingsList, UISettingsRoot.DisplaySettings);
			InitializeSettingsList(m_AccessiablitySettingsList, UISettingsRoot.AccessiabilitySettings);
			InitializeSettingsList(m_LanguageFirstLaunchSettingsList, UISettingsRoot.GameSettings);
			InitializeSettingsList(m_GraphicsSettingsList, UISettingsRoot.GraphicsSettings);
			InitializeSettingsList(m_ControlSettingsList, UISettingsRoot.Controls);
			InitializeSettingsList(m_StartGameDifficultySettingsList, UISettingsRoot.StartGame);
			InitializeSettingsList(m_DevelopmentSettingsList, UISettingsRoot.Development);
			BuildKeyBindCache();
		}
	}

	private void InitializeSettingsList(List<UISettingsGroup> settingsList, UISettingsGroup[] groups)
	{
		settingsList.Clear();
		foreach (UISettingsGroup uISettingsGroup in groups)
		{
			if (!(uISettingsGroup == null))
			{
				settingsList.Add(uISettingsGroup);
			}
		}
	}

	public List<UISettingsGroup> GetSettingsList(SettingsScreen? screenId)
	{
		if (!screenId.HasValue)
		{
			return null;
		}
		Initialize();
		return screenId switch
		{
			SettingsScreen.Game => m_GameSettingsList, 
			SettingsScreen.Graphics => m_GraphicsSettingsList, 
			SettingsScreen.Difficulty => m_DifficultySettingsList, 
			SettingsScreen.Sound => m_SoundSettingsList, 
			SettingsScreen.Controls => m_ControlSettingsList, 
			SettingsScreen.StartGameDifficulty => m_StartGameDifficultySettingsList, 
			SettingsScreen.Display => m_DisplaySettingsList, 
			SettingsScreen.Accessiability => m_AccessiablitySettingsList, 
			SettingsScreen.Language => m_LanguageFirstLaunchSettingsList, 
			SettingsScreen.SafeZone => m_SafeZoneSettingsList, 
			SettingsScreen.Development => m_DevelopmentSettingsList, 
			_ => null, 
		};
	}

	public void OnSettingsApplied()
	{
		Game.Instance.Player.MinDifficultyController.UpdateMinDifficulty();
	}
}
