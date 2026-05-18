using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Code.UI.MVVM;

public class InteractionNavigationController : IController, IControllerTick, IControllerDisable
{
	private readonly List<Entity> m_Available = new List<Entity>();

	private readonly InteractableFilter m_Filter = new InteractableFilter();

	private readonly NearestSorter m_NearestSorter = new NearestSorter();

	private readonly NavigationSorter m_NavigationSorter = new NavigationSorter(NavigationSorter.Mode.ScreenSpaceX);

	private Entity m_NavigationEntity;

	public Entity Focus => m_NavigationEntity ?? m_Available.FirstOrDefault();

	private void Update(BaseUnitEntity interactor, Camera camera)
	{
		if (Game.Instance.CursorController.IsCursorActive)
		{
			Clear();
			return;
		}
		m_Filter.Reset(interactor);
		m_NearestSorter.Reset(interactor);
		List<Entity> value;
		using (CollectionPool<List<Entity>, Entity>.Get(out value))
		{
			InteractionNavigationUtility.GetEntitiesNonAlloc(interactor.Position, 12.7f, m_Filter.IsMatch, m_NearestSorter, value);
			if (!InteractionNavigationUtility.AreEqual(m_Available, value))
			{
				if (m_NavigationEntity != null && !value.Contains(m_NavigationEntity))
				{
					m_NavigationEntity = null;
				}
				Entity entity = m_NavigationEntity ?? value.FirstOrDefault();
				InteractionNavigationUtility.Notify(m_Available, value, entity);
				m_Available.Clear();
				m_Available.AddRange(value);
				m_NavigationSorter.Reset(interactor, entity, camera);
			}
		}
	}

	private void Clear()
	{
		List<Entity> value;
		using (CollectionPool<List<Entity>, Entity>.Get(out value))
		{
			m_NavigationEntity = null;
			InteractionNavigationUtility.Notify(m_Available, value, m_NavigationEntity);
			m_Available.Clear();
		}
	}

	public void Navigate(int indexOffset)
	{
		if (m_Available.Count <= 1)
		{
			return;
		}
		List<Entity> value;
		using (CollectionPool<List<Entity>, Entity>.Get(out value))
		{
			value.AddRange(m_Available);
			value.Sort(m_NavigationSorter);
			Entity item = m_NavigationEntity ?? m_Available.FirstOrDefault();
			int index = InteractionNavigationUtility.Wrap(value.IndexOf(item) + indexOffset, value.Count);
			Navigate(value[index]);
		}
	}

	public void Navigate(Entity entity)
	{
		if (m_Available.Contains(entity))
		{
			m_NavigationEntity = entity;
			InteractionNavigationUtility.Notify(m_Available, m_Available, m_NavigationEntity);
		}
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.BeginOfFrame;
	}

	void IControllerTick.Tick()
	{
		BaseUnitEntity value = Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value;
		Camera camera = CameraRig.Instance.Camera;
		if (value != null && camera != null)
		{
			Update(value, camera);
		}
		else
		{
			Clear();
		}
	}

	void IControllerDisable.OnDisable()
	{
		Clear();
	}
}
