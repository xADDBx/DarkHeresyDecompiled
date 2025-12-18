using UnityEngine;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[CreateAssetMenu(menuName = "Settings UI/On Off Toggle")]
public class UISettingsEntityBool : UISettingsEntityWithValueBase<bool>
{
	public bool DefaultValue;

	public override SettingsListItemType? Type => SettingsListItemType.OnOffToggle;
}
