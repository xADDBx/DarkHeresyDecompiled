using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD;

public abstract class XPBDEntity : MonoBehaviour
{
	private int m_IndexInSolver = -1;

	public Action<XPBDEntity> IndexInSolverChanged;

	public bool IsActiveInSimulation => m_IndexInSolver > -1;

	public int IndexInSolver
	{
		get
		{
			return m_IndexInSolver;
		}
		internal set
		{
			if (m_IndexInSolver != value)
			{
				m_IndexInSolver = value;
				OnIndexInSolverChanged();
				IndexInSolverChanged?.Invoke(this);
			}
		}
	}

	public bool IsValid => this != null;

	public virtual Transform GetTransform()
	{
		if (IsValid)
		{
			return base.transform;
		}
		return null;
	}

	private void OnIndexInSolverChanged()
	{
		if (m_IndexInSolver == -1)
		{
			OnUnregister();
		}
		else
		{
			OnRegister();
		}
	}

	protected virtual void OnUnregister()
	{
	}

	protected virtual void OnRegister()
	{
	}
}
