using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatUnitPortraitWidget : MonoBehaviour
{
	[SerializeField]
	private SurfaceCombatUnitPortraitZone m_PersonalPortrait;

	[SerializeField]
	private SurfaceCombatUnitPortraitZone m_SubtypePortrait;

	[SerializeField]
	private OwlcatMultiSelectable m_Multiselectable;

	public void SetVisualLayer(bool isEnemy, bool isParty)
	{
		string activeLayer = (isEnemy ? "Enemy" : (isParty ? "Party" : "Ally"));
		m_Multiselectable.SetActiveLayer(activeLayer);
	}

	public void SetPortrait(MechanicEntity entity, bool hasPersonalPortrait)
	{
		if (hasPersonalPortrait)
		{
			SetPersonalPortrait(entity);
		}
		else
		{
			SetSubtypePortrait(entity);
		}
	}

	public void ClearPortrait()
	{
		m_PersonalPortrait.Hide();
		m_SubtypePortrait.Hide();
	}

	private void SetPersonalPortrait(MechanicEntity entity)
	{
		m_SubtypePortrait.Hide();
		m_PersonalPortrait.SetUnit(entity);
	}

	private void SetSubtypePortrait(MechanicEntity entity)
	{
		m_PersonalPortrait.Hide();
		m_SubtypePortrait.SetUnit(entity);
	}
}
