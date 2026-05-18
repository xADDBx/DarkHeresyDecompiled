namespace Kingmaker.Code.UI.MVVM;

public class BrickChargenStatTitleVM : TooltipBrickVM
{
	public readonly string DisplayName;

	public readonly string Subname;

	public readonly string Acronym;

	public readonly string Value;

	public BrickChargenStatTitleVM(string displayName, string subname, string acronym, string value)
	{
		DisplayName = displayName;
		Subname = subname;
		Acronym = acronym;
		Value = value;
	}
}
