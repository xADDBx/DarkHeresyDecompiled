using DG.Tweening;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.DragNDrop;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetRolesPlayerCharacterBaseView : View<NetRolesPlayerCharacterVM>, INetRoleHighlight, ISubscriber
{
	[SerializeField]
	protected Image m_Portrait;

	public PhotonActorNumber PlayerOwner => base.ViewModel.PlayerOwner;

	public UnitReference Character => base.ViewModel.Character;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		EventBus.Subscribe(this).AddTo(this);
		base.ViewModel.PlayerRoleMe.Subscribe(m_Portrait.gameObject.SetActive).AddTo(this);
		base.ViewModel.Portrait.Subscribe(delegate(Sprite value)
		{
			base.gameObject.SetActive(value != null);
			if ((bool)value)
			{
				m_Portrait.sprite = value;
			}
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		DragNDropManager.Instance.Or(null)?.CancelDrag();
	}

	public void HandleRoleHighlight(UnitReference unit, bool highlight)
	{
		CanvasGroup target = this.EnsureComponent<CanvasGroup>();
		if (highlight)
		{
			target.DOFade(unit.Equals(Character) ? 1f : 0.5f, 0.2f);
		}
		else
		{
			target.DOFade(1f, 0.2f);
		}
	}
}
