namespace Kingmaker.Framework.DetectiveSystem;

public interface IHasDetectiveCaseItemCondition
{
	BlueprintCaseItem CaseItem { get; }

	bool Not { get; }
}
