using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.DragNDrop;
using Owlcat.UI;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetRolesPlayerCharacterPCView : NetRolesPlayerCharacterBaseView, IDraggableElement
{
	[SerializeField]
	private OwlcatButton m_MoveRoleUp;

	[SerializeField]
	private OwlcatButton m_MoveRoleDown;

	[SerializeField]
	private RectTransform m_MoveRoleUpBackground;

	[SerializeField]
	private RectTransform m_MoveRoleDownBackground;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.CanUp.Subscribe(delegate(bool value)
		{
			m_MoveRoleUp.gameObject.SetActive(value);
			m_MoveRoleUpBackground.gameObject.SetActive(value);
		}).AddTo(this);
		base.ViewModel.CanDown.Subscribe(delegate(bool value)
		{
			m_MoveRoleDown.gameObject.SetActive(value);
			m_MoveRoleDownBackground.gameObject.SetActive(value);
		}).AddTo(this);
		m_MoveRoleUp.OnLeftClickAsObservable().Subscribe(base.ViewModel.MoveRoleCharacterUp).AddTo(this);
		m_MoveRoleDown.OnLeftClickAsObservable().Subscribe(base.ViewModel.MoveRoleCharacterDown).AddTo(this);
		if (PhotonManager.Instance.IsRoomOwner)
		{
			this.OnBeginDragAsObservable().Subscribe(OnBeginDrag).AddTo(this);
			this.OnDragAsObservable().Subscribe(OnDrag).AddTo(this);
			this.OnEndDragAsObservable().Subscribe(OnEndDrag).AddTo(this);
			this.OnDropAsObservable().Subscribe(OnDrop).AddTo(this);
		}
	}

	private void MoveCharacter(PhotonActorNumber player)
	{
		base.ViewModel.MoveCharacter(player);
	}

	private void OnBeginDrag(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnBeginDrag(eventData, base.gameObject);
		});
	}

	private void OnDrag(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnDrag(eventData);
		});
	}

	private void OnEndDrag(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnEndDrag(eventData);
		});
	}

	private void OnDrop(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnDrop(eventData, base.gameObject);
		});
	}

	public void StartDrag()
	{
		EventBus.RaiseEvent(delegate(INetRoleHighlight h)
		{
			h.HandleRoleHighlight(base.Character, highlight: true);
		});
	}

	public void EndDrag(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(INetRoleHighlight h)
		{
			h.HandleRoleHighlight(base.Character, highlight: false);
		});
		GameObject dropTarget = DragNDropManager.DropTarget;
		if (!(dropTarget == null))
		{
			NetRolesPlayerCharacterBaseView component = dropTarget.GetComponent<NetRolesPlayerCharacterBaseView>();
			if (component != null && component.Character.Equals(base.Character))
			{
				MoveCharacter(component.PlayerOwner);
			}
		}
	}

	public bool SetDragSlot(DragNDropManager slot)
	{
		if (!base.ViewModel.PlayerRoleMe.CurrentValue)
		{
			return false;
		}
		slot.Icon.sprite = base.ViewModel.Portrait.CurrentValue;
		slot.Count.text = string.Empty;
		slot.OverideSize = new Vector2(81f, 106f);
		return true;
	}

	public void CancelDrag()
	{
	}
}
