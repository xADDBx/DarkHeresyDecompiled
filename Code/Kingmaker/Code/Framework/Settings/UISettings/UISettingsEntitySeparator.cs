using UnityEngine;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[CreateAssetMenu(menuName = "Settings UI/Separator")]
public class UISettingsEntitySeparator : UISettingsEntityBase
{
	public override SettingsListItemType? Type => SettingsListItemType.Separator;
}
