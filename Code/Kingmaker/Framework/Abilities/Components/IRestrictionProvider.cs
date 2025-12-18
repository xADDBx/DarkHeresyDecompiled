using Kingmaker.Designers.Mechanics.Facts.Restrictions;

namespace Kingmaker.Framework.Abilities.Components;

public interface IRestrictionProvider
{
	RestrictionCalculator GetRestriction();
}
