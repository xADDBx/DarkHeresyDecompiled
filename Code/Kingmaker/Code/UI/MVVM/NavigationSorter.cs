using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

internal class NavigationSorter : IComparer<Entity>
{
	public enum Mode
	{
		CrossProduct,
		Clockwise,
		ScreenSpaceX
	}

	private readonly Mode m_Mode;

	private Entity m_Unit;

	private Entity m_Selection;

	private Camera m_Camera;

	public NavigationSorter(Mode mode)
	{
		m_Mode = mode;
	}

	public void Reset(Entity unit, Entity selection, Camera camera)
	{
		m_Unit = unit;
		m_Selection = selection;
		m_Camera = camera;
	}

	public void Reset(Entity selection)
	{
		m_Selection = selection;
	}

	public int Compare(Entity x, Entity y)
	{
		return GetScore(x).CompareTo(GetScore(y));
	}

	private float GetScore(Entity entity)
	{
		return m_Mode switch
		{
			Mode.CrossProduct => GetCrossProduct2D(entity), 
			Mode.Clockwise => GetAngle(entity), 
			Mode.ScreenSpaceX => GetScreenSpaceX(entity), 
			_ => throw new NotImplementedException(), 
		};
	}

	private float GetScreenSpaceX(Entity entity)
	{
		return m_Camera.WorldToScreenPoint(entity.Position).x;
	}

	private float GetAngle(Entity target)
	{
		Vector3 vector = target.Position - m_Unit.Position;
		return Mathf.Atan2(vector.x, vector.z);
	}

	private float GetCrossProduct2D(Entity entity)
	{
		Vector3 position = m_Unit.Position;
		Vector3 position2 = m_Selection.Position;
		Vector3 position3 = entity.Position;
		return (position3.x - position.x) * (position2.z - position.z) - (position3.z - position.z) * (position2.x - position.x);
	}
}
