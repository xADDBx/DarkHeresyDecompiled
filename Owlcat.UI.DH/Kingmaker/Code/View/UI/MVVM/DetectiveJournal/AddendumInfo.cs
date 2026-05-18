using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class AddendumInfo : InfoWrapper
{
	public readonly BlueprintClueAddendum BlueprintAddendum;

	protected override int HashCode => BlueprintAddendum.AssetGuid.GetHashCode();

	public AddendumInfo(BlueprintClueAddendum blueprintAddendum)
	{
		BlueprintAddendum = blueprintAddendum;
	}

	public override void MarkAsViewed()
	{
		(from a in Game.Instance.DetectiveSystem.GetOpenedAddendumsFor(BlueprintAddendum.ParentClue)
			where a.HasOverride(BlueprintAddendum)
			select a).ForEach(UIUtilityDetective.ExaminedDetectiveData.ExaminedAddendums.AddExaminedEntityIfNeeded);
		UIUtilityDetective.ExaminedDetectiveData.ExaminedAddendums.AddExaminedEntityIfNeeded(BlueprintAddendum);
		EventBus.RaiseEvent(delegate(INewInfoHandler h)
		{
			h.HandleMarkAsViewed(this);
		});
	}

	public override void RefreshData()
	{
		foreach (BlueprintClue clue in UIUtilityDetective.CollectCluesAffectedByAddendum(BlueprintAddendum))
		{
			EventBus.RaiseEvent(delegate(IClueDataChangedHandler h)
			{
				h.RefreshDataFor(clue);
			});
		}
	}

	public override AddendumState GetAddendumState()
	{
		if (!UIUtilityDetective.ExaminedDetectiveData.ExaminedAddendums.IsEntityNew(BlueprintAddendum))
		{
			return AddendumState.Default;
		}
		return AddendumState.New;
	}

	public override bool Equals(InfoWrapper other)
	{
		if (other is AddendumInfo addendumInfo)
		{
			return addendumInfo.BlueprintAddendum == BlueprintAddendum;
		}
		return false;
	}
}
