using JetBrains.Annotations;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class FormationCharacterBaseView : View<FormationCharacterVM>
{
	[SerializeField]
	[UsedImplicitly]
	protected OwlcatButton m_Button;

	[SerializeField]
	[UsedImplicitly]
	private Image m_Portrait;

	[SerializeField]
	[UsedImplicitly]
	private Color m_GreyColor;

	[SerializeField]
	[UsedImplicitly]
	protected FormationCharacterDragComponent m_FormationCharacterDragComponent;

	protected override void OnBind()
	{
		m_FormationCharacterDragComponent.Initialize(base.transform.parent as RectTransform);
		m_Portrait.sprite = base.ViewModel.PortraitSprite;
		SetupPosition();
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.FormationUpdated, delegate
		{
			SetupPosition();
		}).AddTo(this);
		base.ViewModel.IsInteractable.Subscribe(SetInteractable).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.ViewModel.MoveCharacter(((Vector2)base.transform.localPosition - base.ViewModel.OffsetPosition) / 40f);
	}

	protected virtual void SetupPosition()
	{
		Vector3 localPosition = base.ViewModel.GetLocalPosition();
		localPosition.x -= localPosition.x % 23f;
		localPosition.y -= localPosition.y % 23f;
		base.transform.localPosition = localPosition;
		if (!(base.ViewModel.GetLocalPosition() == localPosition))
		{
			base.ViewModel.MoveCharacter(((Vector2)localPosition - base.ViewModel.OffsetPosition) / 40f);
		}
	}

	private void SetInteractable(bool value)
	{
		m_Button.Interactable = value;
		m_FormationCharacterDragComponent.IsInteractable = value;
		m_Portrait.color = (value ? Color.white : m_GreyColor);
	}
}
