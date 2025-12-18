using UnityEngine;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[CreateAssetMenu(menuName = "Settings UI/ResetControllerMode")]
public class UISettingsEntityResetControllerMode : UISettingsEntityBase
{
	public override SettingsListItemType? Type => SettingsListItemType.Custom;
}
