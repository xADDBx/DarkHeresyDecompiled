using System;
using DG.Tweening;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public interface ITurnVirtualItemView
{
	MonoBehaviour View { get; }

	OwlcatSelectable Selectable { get; }

	RectTransform RectTransform { get; }

	CanvasGroup CanvasGroup { get; }

	bool WillBeReused { get; set; }

	ViewModel GetViewModel();

	void ViewBind(ITurnVirtualItemData viewModel);

	Tween GetHideAnimation(Action completeAction);

	Tween GetMoveAnimation(Action completeAction, Vector2 targetPosition);

	Tween GetShowAnimation(Action completeAction, Vector2 targetPosition);

	void SetAnchoredPosition(Vector2 position);

	void DestroyViewItem();
}
