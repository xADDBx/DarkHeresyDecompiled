using Owlcat.Runtime.Visual.XPBD.Bodies;
using Owlcat.Runtime.Visual.XPBD.Constraints;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Layouts;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Authoring;

public abstract class AuthoringBase : XPBDEntity
{
	private int m_Layer;

	private int m_LastParametersHash;

	private int m_LastConstraintsSettingsHash;

	[SerializeField]
	[HideInInspector]
	private ConstraintSettingsCollection m_ConstraintSettings = ConstraintSettingsCollection.Default;

	protected Aabb m_BodyAabb;

	private bool m_IsVisible;

	[SerializeField]
	[HideInInspector]
	private BodySimulationParameters m_BodySimulationParameters = BodySimulationParameters.Default;

	public abstract LayoutBase LayoutBase { get; }

	public ConstraintSettingsCollection ConstraintSettings => m_ConstraintSettings;

	public Aabb BodyAabb => m_BodyAabb;

	public BodySimulationParameters BodySimulationParameters => m_BodySimulationParameters;

	public int Layer => m_Layer;

	private void Awake()
	{
		if (TryInitialize())
		{
			XPBD.RegisterAuthoring(this);
			OnAfterAwake();
		}
	}

	protected virtual bool TryInitialize()
	{
		return LayoutBase != null;
	}

	protected virtual void OnAfterAwake()
	{
	}

	private void OnDestroy()
	{
		if (LayoutBase != null)
		{
			XPBD.UnregisterAuthoring(this);
		}
		OnAfterDestroy();
	}

	protected virtual void OnAfterDestroy()
	{
	}

	private void OnEnable()
	{
		XPBD.SyncAuthoringEnabledState(this);
	}

	private void OnDisable()
	{
		XPBD.SyncAuthoringEnabledState(this);
	}

	internal virtual void AfterEnabledStateSync(Solver solver)
	{
	}

	internal void PrepareFrame(Solver solver)
	{
		if (base.IsValid)
		{
			PrepareFrameInternal(solver);
		}
	}

	protected void PrepareFrameInternal(Solver solver)
	{
		if (m_Layer != base.gameObject.layer)
		{
			m_Layer = base.gameObject.layer;
			solver.UpdateLayer(this);
		}
		int num = m_ConstraintSettings.CalculateHash();
		if (m_LastConstraintsSettingsHash != num)
		{
			solver.UpdateConstraintSettings(this);
			m_LastConstraintsSettingsHash = num;
		}
		int num2 = m_BodySimulationParameters.CalculateHash();
		if (m_LastParametersHash != num2)
		{
			solver.UpdateBodySimulationParameters(this);
			m_LastParametersHash = num2;
		}
	}

	public void BeginStep(Solver solver)
	{
		if (base.IsValid)
		{
			BeginStepInternal(solver);
		}
	}

	protected virtual void BeginStepInternal(Solver solver)
	{
		m_IsVisible = solver.GetVisibility(this);
	}

	internal void EndStep(Solver solver)
	{
		if (base.IsValid)
		{
			EndStepInternal(solver);
		}
	}

	protected virtual void EndStepInternal(Solver solver)
	{
		solver.GetBodyAabb(this, out m_BodyAabb);
	}

	public uint GetEnabledConstraintTypeMask()
	{
		uint usedConstraints = LayoutBase.BodyStructure.UsedConstraints;
		uint enabledMask = ConstraintSettings.GetEnabledMask();
		return usedConstraints & enabledMask;
	}

	internal virtual void OnDrawGizmosSelected()
	{
		Color color = Gizmos.color;
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(m_BodyAabb.Center, m_BodyAabb.Size);
		Gizmos.color = color;
	}
}
public abstract class AuthoringBase<T> : AuthoringBase where T : LayoutBase
{
	[SerializeField]
	private T m_Layout;

	public override LayoutBase LayoutBase => m_Layout;

	public T Layout
	{
		get
		{
			return m_Layout;
		}
		set
		{
			if (m_Layout != value)
			{
				m_Layout = value;
				if (base.IsActiveInSimulation)
				{
					XPBD.ResetAuthoring(this);
				}
			}
		}
	}
}
