using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class StudyInfo : InfoWrapper
{
	public readonly BlueprintClueStudy BlueprintStudy;

	protected override int HashCode => BlueprintStudy.AssetGuid.GetHashCode();

	public StudyInfo(BlueprintClueStudy blueprintStudy)
	{
		BlueprintStudy = blueprintStudy;
	}

	public override void MarkAsViewed()
	{
		UIUtilityDetective.ExaminedDetectiveData.ExaminedStudies.AddExaminedEntityIfNeeded(BlueprintStudy);
		EventBus.RaiseEvent(delegate(INewInfoHandler h)
		{
			h.HandleMarkAsViewed(this);
		});
	}

	public override void RefreshData()
	{
		EventBus.RaiseEvent(delegate(IClueDataChangedHandler h)
		{
			h.RefreshDataFor(BlueprintStudy.ParentClue);
		});
	}

	public override AddendumState GetAddendumState()
	{
		if (!UIUtilityDetective.ExaminedDetectiveData.ExaminedStudies.IsEntityNew(BlueprintStudy))
		{
			return AddendumState.Default;
		}
		return AddendumState.New;
	}

	public override bool Equals(InfoWrapper other)
	{
		if (other is StudyInfo studyInfo)
		{
			return studyInfo.BlueprintStudy == BlueprintStudy;
		}
		return false;
	}
}
