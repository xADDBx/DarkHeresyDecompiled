using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SurfaceCombatUnitPortraitZone : MonoBehaviour
{
	[SerializeField]
	private Image m_Picture;

	[SerializeField]
	private PortraitCombatSize m_Size = PortraitCombatSize.Small;

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		m_Picture.sprite = null;
	}

	public void SetUnit(MechanicEntity unit)
	{
		base.gameObject.SetActive(value: true);
		MechanicEntityUIWrapper mechanicEntityUIWrapper = new MechanicEntityUIWrapper(unit);
		Image picture = m_Picture;
		picture.sprite = m_Size switch
		{
			PortraitCombatSize.Icon => mechanicEntityUIWrapper.Icon, 
			PortraitCombatSize.Small => mechanicEntityUIWrapper.SmallPortrait, 
			PortraitCombatSize.Middle => mechanicEntityUIWrapper.MiddlePortrait, 
			_ => mechanicEntityUIWrapper.SmallPortrait, 
		};
	}
}
