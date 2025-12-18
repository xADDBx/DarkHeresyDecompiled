namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCommonPhaseRoadmapView : CharGenPhaseRoadmapView<CharGenPhaseBaseVM>
{
	private bool isInited;

	protected override void BindViewImplementation()
	{
		if (!isInited)
		{
			Initialize();
			isInited = true;
		}
		base.BindViewImplementation();
	}
}
