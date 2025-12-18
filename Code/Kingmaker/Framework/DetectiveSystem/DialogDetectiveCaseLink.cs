namespace Kingmaker.Framework.DetectiveSystem;

public readonly struct DialogDetectiveCaseLink
{
	public readonly BlueprintCase Case;

	public readonly BlueprintCaseItem? Item;

	public DialogDetectiveCaseLink(BlueprintCaseItem item)
	{
		Case = item.ParentCase.Blueprint;
		Item = item;
	}

	public DialogDetectiveCaseLink(BlueprintCase @case)
	{
		Case = @case;
		Item = null;
	}
}
