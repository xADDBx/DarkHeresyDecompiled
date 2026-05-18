using System.Collections.Generic;
using Kingmaker.UnitLogic;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTriggeredAutoVM : TooltipBrickVM
{
	public readonly string TriggeredAutoText;

	public readonly List<ReasonBuffItemVM> ReasonBuffItems = new List<ReasonBuffItemVM>();

	public readonly bool IsSuccess;

	public BrickTriggeredAutoVM(string triggeredAutoText, IReadOnlyList<FeatureCountableFlag.FactsList.Element> reasonItems, bool isSuccess)
	{
		TriggeredAutoText = triggeredAutoText;
		IsSuccess = isSuccess;
		if (reasonItems == null)
		{
			return;
		}
		foreach (FeatureCountableFlag.FactsList.Element reasonItem in reasonItems)
		{
			ReasonBuffItemVM item = new ReasonBuffItemVM(reasonItem.BuffInformation);
			ReasonBuffItems.Add(item);
		}
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		ReasonBuffItems.ForEach(delegate(ReasonBuffItemVM item)
		{
			item.Dispose();
		});
		ReasonBuffItems.Clear();
	}
}
