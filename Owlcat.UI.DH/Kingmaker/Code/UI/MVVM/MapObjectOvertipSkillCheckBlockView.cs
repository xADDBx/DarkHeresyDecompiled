using Code.Framework.Utility.UnityExtensions;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class MapObjectOvertipSkillCheckBlockView : View<OvertipMapObjectVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_DescriptionText;

	[SerializeField]
	private TextMeshProUGUI m_SkillCheckText;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	protected override void OnBind()
	{
		base.ViewModel.ObjectDescription.Subscribe(delegate(string text)
		{
			m_DescriptionText.text = text;
		}).AddTo(this);
		base.ViewModel.ObjectSkillCheckText.Subscribe(delegate(string text)
		{
			m_SkillCheckText.text = text;
		}).AddTo(this);
		base.ViewModel.IsEnabled.CombineLatest(base.ViewModel.MapObjectIsHighlighted, base.ViewModel.ForceHotKeyPressed, base.ViewModel.IsMouseOverUI, base.ViewModel.ObjectDescription, base.ViewModel.ObjectSkillCheckText, base.ViewModel.ForceHideInCombat, (bool enable, bool hover, bool force, bool mouseOver, string descr, string skillcheck, bool forceHideInCombat) => enable && !forceHideInCombat && (hover || force || mouseOver) && (!descr.IsNullOrEmpty() || !skillcheck.IsNullOrEmpty())).Subscribe(delegate(bool value)
		{
			bool value2 = value && Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value.IsMyNetRole();
			m_FadeAnimator.PlayAnimation(value2);
		}).AddTo(this);
	}
}
