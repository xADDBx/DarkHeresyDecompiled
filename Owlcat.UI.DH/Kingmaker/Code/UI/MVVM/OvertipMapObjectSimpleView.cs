using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class OvertipMapObjectSimpleView : BaseOvertipMapObjectView
{
	[Header("Parts Block")]
	[SerializeField]
	private OvertipBarkBlockView m_BarkBlockPCView;

	[SerializeField]
	private MapObjectOvertipNameBlockView m_OvertipNameBlockPCView;

	[SerializeField]
	private MapObjectOvertipSkillCheckBlockView m_OvertipSkillCheckBlockPCView;

	[Header("Common Block")]
	[SerializeField]
	private CanvasGroup m_InnerCanvasGroup;

	protected bool CheckVisibleTrigger
	{
		get
		{
			if (!base.ViewModel.IsMouseOverUI.CurrentValue && !base.ViewModel.MapObjectIsHighlighted.CurrentValue && !base.ViewModel.ForceHotKeyPressed.CurrentValue && !base.ViewModel.ActiveCharacterIsNear && !base.ViewModel.IsBarkActive.CurrentValue && !base.ViewModel.HasSurrounding.CurrentValue)
			{
				return base.ViewModel.IsChosen.CurrentValue;
			}
			return true;
		}
	}

	protected override bool CheckVisibility
	{
		get
		{
			if (base.CheckCanBeVisible)
			{
				return CheckVisibleTrigger;
			}
			return false;
		}
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.gameObject.name = base.ViewModel.MapObjectEntity.View.gameObject.name + "_OvertipMapObjectSimpleView";
		m_BarkBlockPCView.Bind(base.ViewModel.BarkBlockVM);
		m_OvertipNameBlockPCView.Bind(base.ViewModel);
		m_OvertipSkillCheckBlockPCView.Bind(base.ViewModel);
		m_InnerCanvasGroup.blocksRaycasts = false;
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_BarkBlockPCView.Unbind();
		m_OvertipNameBlockPCView.Unbind();
		m_OvertipSkillCheckBlockPCView.Unbind();
	}
}
