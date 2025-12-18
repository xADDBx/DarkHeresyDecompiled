using UnityEngine;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[CreateAssetMenu(menuName = "Settings UI/On Off Toggle Only One Save")]
public class UISettingsEntityBoolOnlyOneSave : UISettingsEntityWithValueBase<bool>
{
	public bool DefaultValue;

	public override SettingsListItemType? Type => SettingsListItemType.OnOffToggle;
}
