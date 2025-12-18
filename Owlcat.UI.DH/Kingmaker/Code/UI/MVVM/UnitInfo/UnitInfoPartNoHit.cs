using Kingmaker.Blueprints.Root.Strings;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartNoHit : UnitInfoPart
{
	[SerializeField]
	private TextMeshProUGUI m_NoHitLabel;

	private void Awake()
	{
		m_NoHitLabel.text = UIStrings.Instance.Tooltips.NoHit.Text;
	}

	protected override void ShowImpl(UnitInfoPartState state)
	{
		base.gameObject.SetActive(state.HasAbility && !state.HasHit && !state.PreciseAttackHasNoTarget && state.HasLineOfSight);
	}
}
