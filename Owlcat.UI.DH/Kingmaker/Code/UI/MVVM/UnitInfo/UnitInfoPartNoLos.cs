using Kingmaker.Blueprints.Root.Strings;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartNoLos : UnitInfoPart
{
	[SerializeField]
	private TextMeshProUGUI m_NoLosLabel;

	private void Awake()
	{
		m_NoLosLabel.text = UIStrings.Instance.Tooltips.NoLineOfSight.Text;
	}

	protected override void ShowImpl(UnitInfoPartState state)
	{
		base.gameObject.SetActive(state.HasAbility && !state.HasHit && !state.PreciseAttackHasNoTarget && !state.HasLineOfSight);
	}
}
