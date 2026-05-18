using System;
using Code.View.UI.Helpers;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UI.Pointer;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class PrerequisiteEntryView : View<PrerequisiteEntryVM>
{
	[Header("Elements")]
	[SerializeField]
	protected TextValueTupleView m_Text;

	[Header("Selectables")]
	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	protected int? m_LinkIndex;

	protected string m_LinkKey;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		m_Text.Bind(base.ViewModel.Info);
		m_StateSelectable.SetActiveLayer(base.ViewModel.Done ? "Done" : "Required");
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			SetLinkHighlight().AddTo(this);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_LinkIndex = null;
		m_LinkKey = null;
	}

	private IDisposable SetLinkHighlight()
	{
		if (m_Text == null)
		{
			return Disposable.Empty;
		}
		bool entered = TMP_TextUtilities.IsIntersectingRectTransform(m_Text.Text.rectTransform, CursorController.CursorPosition, UICamera.Claim());
		IDisposable enter = m_Text.OnPointerEnterAsObservable().Subscribe(delegate
		{
			entered = true;
		});
		IDisposable exit = m_Text.OnPointerExitAsObservable().Subscribe(delegate
		{
			entered = false;
		});
		IDisposable update = ObservableSubscribeExtensions.Subscribe(m_Text.UpdateAsObservable(), delegate
		{
			if (!entered)
			{
				ClearFocusIfNeeded();
				ClearLinkIndexIfNeeded();
			}
			else
			{
				int num = TMP_TextUtilities.FindIntersectingLink(m_Text.Text, CursorController.CursorPosition, UICamera.Claim());
				if (num == -1)
				{
					ClearFocusIfNeeded();
					ClearLinkIndexIfNeeded();
				}
				else if (num != m_LinkIndex)
				{
					m_LinkIndex = num;
					m_LinkKey = UtilityLink.GetKeysFromLink(m_Text.Text.textInfo.linkInfo[m_LinkIndex.Value].GetLinkID())[1];
					EventBus.RaiseEvent(delegate(IUIHighlighter h)
					{
						h.StartHighlight(m_LinkKey);
					});
				}
			}
		});
		IDisposable click = m_Text.OnPointerClickAsObservable().Subscribe(delegate(PointerEventData data)
		{
			if (m_LinkIndex.HasValue && data.button == PointerEventData.InputButton.Right)
			{
				HighlightCurrentOnce();
			}
		});
		return Disposable.Create(delegate
		{
			enter.Dispose();
			exit.Dispose();
			update.Dispose();
			click.Dispose();
		});
	}

	protected void HighlightCurrentOnce()
	{
		if (!string.IsNullOrEmpty(m_LinkKey))
		{
			EventBus.RaiseEvent(delegate(IUIHighlighter h)
			{
				h.HighlightOnce(m_LinkKey);
			});
		}
	}

	private void ClearFocusIfNeeded()
	{
	}

	protected void ClearLinkIndexIfNeeded()
	{
		int? linkIndex = m_LinkIndex;
		string linkKey = m_LinkKey;
		m_LinkIndex = null;
		m_LinkKey = null;
		if (linkIndex.HasValue && !string.IsNullOrEmpty(linkKey))
		{
			EventBus.RaiseEvent(delegate(IUIHighlighter h)
			{
				h.StopHighlight(m_LinkKey);
			});
		}
	}
}
