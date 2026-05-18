namespace Kingmaker.Code.UI.MVVM;

public readonly struct AbilityRestrictionEntry
{
	public readonly string Description;

	public readonly bool IsPassed;

	public AbilityRestrictionEntry(string description, bool isPassed)
	{
		Description = description;
		IsPassed = isPassed;
	}
}
