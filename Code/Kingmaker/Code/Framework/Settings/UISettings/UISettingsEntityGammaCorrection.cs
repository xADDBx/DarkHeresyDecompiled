using UnityEngine;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[CreateAssetMenu(menuName = "Settings UI/GammaCorrection")]
public class UISettingsEntityGammaCorrection : UISettingsEntitySliderFloat
{
	public override SettingsListItemType? Type => SettingsListItemType.Custom;
}
