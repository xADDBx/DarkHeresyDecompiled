using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public class EntityInfoView : View<EntityInfoVM>
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private BaseEntityInfoWidget[] m_InfoWidgets;

	protected override void OnBind()
	{
		base.ViewModel.EntityInfo.Subscribe(HandleInfoChanged).AddTo(this);
	}

	private void HandleInfoChanged(IEntityInfo entityInfo)
	{
		bool flag = false;
		BaseEntityInfoWidget[] infoWidgets = m_InfoWidgets;
		foreach (BaseEntityInfoWidget baseEntityInfoWidget in infoWidgets)
		{
			if (flag)
			{
				baseEntityInfoWidget.Hide();
			}
			else if (baseEntityInfoWidget.TryShow(entityInfo))
			{
				flag = true;
			}
		}
		m_FadeAnimator.PlayAnimation(flag);
	}
}
