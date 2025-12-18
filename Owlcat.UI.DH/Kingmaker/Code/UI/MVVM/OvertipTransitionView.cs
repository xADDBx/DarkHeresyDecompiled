using Code.Framework.Utility.UnityExtensions;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class OvertipTransitionView : BaseOvertipView<OvertipTransitionVM>
{
	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[SerializeField]
	private CanvasGroup m_TitleBlock;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private CanvasGroup m_ActiveLayer;

	protected override bool CheckVisibility
	{
		get
		{
			if (base.ViewModel.MapObjectEntity.IsVisibleForPlayer && !base.ViewModel.MapObjectEntity.Suppressed && !base.ViewModel.IsCutscene)
			{
				if (Game.Instance.Player.IsInCombat)
				{
					if (Game.Instance.Player.IsInCombat)
					{
						return base.ViewModel.EnableInCombat;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	protected override void OnBind()
	{
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClick();
		}).AddTo(this);
		m_Title.autoSizeTextContainer = false;
		m_Title.autoSizeTextContainer = true;
		base.gameObject.name = base.ViewModel.MapObjectEntity.View.gameObject.name + "_TransitionOvertip";
		base.ViewModel.HasCharactersMovingToHere.Subscribe(delegate(bool value)
		{
			m_ActiveLayer.alpha = (value ? 1 : 0);
		}).AddTo(this);
		base.ViewModel.Title.Subscribe(delegate(string value)
		{
			m_TitleBlock.alpha = ((!value.IsNullOrEmpty()) ? 1 : 0);
			m_Title.text = value;
		}).AddTo(this);
	}
}
