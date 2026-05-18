using UnityEngine;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[CreateAssetMenu(menuName = "Blueprints/Settings UI/ColorList")]
public class UIColorList : ScriptableObject
{
	public Color32[] Colors = new Color32[0];
}
