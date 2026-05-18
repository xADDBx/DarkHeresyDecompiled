namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCommonPhaseRoadmapView : CharGenPhaseRoadmapView<CharGenPhaseBaseVM>
{
	private bool m_IsInited;

	protected override void BindViewImplementation()
	{
		if (!m_IsInited)
		{
			Initialize();
			m_IsInited = true;
		}
		base.BindViewImplementation();
	}
}
