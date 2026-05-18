using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Controllers.Footprints;

internal class UnitFootprintState
{
	public bool InParty;

	public Vector3 PreviousFootPosition;

	public List<Footprint> ActiveFootprints;

	public FootLocator? PendingFoot;

	public int PendingFootIndex;
}
