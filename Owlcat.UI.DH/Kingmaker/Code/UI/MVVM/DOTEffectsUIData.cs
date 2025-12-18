using System.Collections.Generic;
using Code.Enums;

namespace Kingmaker.Code.UI.MVVM;

public struct DOTEffectsUIData
{
	public IReadOnlyList<(DOT dotType, int rank)> DotEffects;
}
