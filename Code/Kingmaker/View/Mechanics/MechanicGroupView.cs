using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.View.Spawners;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Mechanics;

public abstract class MechanicGroupView<T> : EntityGroupViewBase where T : MechanicEntityView
{
	[HashNoGenerate]
	[OwlPackable(OwlPackableMode.Generate)]
	public abstract class MechanicGroupData : SimpleEntity, IOwlPackable<MechanicGroupData>
	{
		[CanBeNull]
		public new MechanicGroupView<T> View => (MechanicGroupView<T>)base.View;

		public T[] ChildrenViews
		{
			get
			{
				if (!View.ComponentsParent)
				{
					return Array.Empty<T>();
				}
				return View.ComponentsParent.GetComponentsInChildren<T>(includeInactive: true);
			}
		}

		public MechanicGroupData(EntityViewBase view)
			: base(view)
		{
		}

		protected MechanicGroupData(JsonConstructorMark _)
			: base(_)
		{
		}

		protected MechanicGroupData()
		{
		}

		protected override void OnIsInGameChanged()
		{
			base.OnIsInGameChanged();
			if (base.IsInGame)
			{
				TryCreateViews();
			}
			SetViewInGame(base.IsInGame);
			if (base.IsInGame)
			{
				View.OnActivate();
			}
			else
			{
				View.OnDeactivate();
			}
		}

		private void SetViewInGame(bool flag)
		{
			if (View.ComponentsParent != null)
			{
				View.ComponentsParent.SetActive(flag);
			}
			T[] childrenViews = ChildrenViews;
			foreach (T val in childrenViews)
			{
				if (!(val == null) && val.Data != null)
				{
					val.Data.IsInGame = flag;
					val.SetVisible(flag);
				}
			}
		}

		private void TryCreateViews()
		{
			T[] childrenViews = ChildrenViews;
			foreach (T val in childrenViews)
			{
				if (!(val == null) && val.Data == null)
				{
					try
					{
						Game.Instance.Controllers.EntitySpawner.SpawnEntityWithView(val, Game.Instance.LoadedAreaState.MainState, moveView: false);
					}
					catch (Exception ex)
					{
						PFLog.SceneLoader.Error($"Exception when creating data for {val} in {val.gameObject.scene.name}, {ex.Message}");
					}
				}
			}
		}

		protected override void OnViewDidAttach()
		{
			base.OnViewDidAttach();
			SetViewInGame(base.IsInGame);
		}

		protected override void OnViewWillDetach()
		{
			base.OnViewWillDetach();
			SetViewInGame(flag: false);
		}
	}

	public GameObject ComponentsParent;

	public override bool CreatesDataOnLoad => true;

	public new MechanicGroupData Data => (MechanicGroupData)base.Data;

	public virtual void Activate(bool flag)
	{
		Data.IsInGame = flag;
	}

	protected virtual void OnActivate()
	{
	}

	protected virtual void OnDeactivate()
	{
	}
}
