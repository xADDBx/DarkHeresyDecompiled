namespace Kingmaker.Framework.DetectiveSystem;

public readonly struct DialogDetectiveCloseCaseData
{
	public readonly BlueprintCase Case;

	public readonly BlueprintCaseAnswer Answer;

	public DialogDetectiveCloseCaseData(BlueprintCase @case, BlueprintCaseAnswer answer)
	{
		Case = @case;
		Answer = answer;
	}
}
