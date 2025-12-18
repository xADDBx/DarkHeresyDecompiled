using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoAbilityView : MonoBehaviour
{
	[SerializeField]
	private Image m_Icon;

	public AbilityUIGroup Group { get; private set; }

	public void Initialize((Sprite icon, AbilityUIGroup group) ability)
	{
		m_Icon.sprite = ability.icon;
		Group = ability.group;
	}
}
