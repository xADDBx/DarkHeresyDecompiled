using System;
using Kingmaker.Tutorial;

namespace Kingmaker.Code.UI.MVVM;

public class TutorialHintWindowVM : TutorialWindowVM
{
	private bool m_IsExpand;

	private EntityVM SolutionEntity { get; }

	public TutorialHintWindowVM(TutorialData data, Action callbackHide)
		: base(data, callbackHide)
	{
		if (data.SolutionFound)
		{
			if (data.SolutionItem != null)
			{
				SolutionEntity = new EntityVM(data.SolutionUnit, data.SolutionItem);
			}
			else if (data.SolutionAbility != null)
			{
				SolutionEntity = new EntityVM(data.SolutionUnit, data.SolutionAbility);
			}
		}
		m_IsExpand = false;
	}

	public void ChangeExpandState()
	{
		m_IsExpand = !m_IsExpand;
	}
}
