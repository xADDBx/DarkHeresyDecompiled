using System;
using Kingmaker.Code.View.Bridge.Root;
using Owlcat.UI;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.ResourceLinks;

[Serializable]
[RequireSeparateBundle]
public class UIViewLink<TView, TViewModel> : WeakResourceLink<TView>, IHashable where TView : ViewBase<TViewModel> where TViewModel : class, IViewModel
{
	public Transform Target;

	private TView m_View;

	private TViewModel m_ViewModel;

	[HideInInspector]
	public Action<TView> CustomInitialize;

	[SerializeField]
	private bool m_HoldAfterInstantiate;

	public void WarmUp()
	{
		TryInstantiatePrefab();
	}

	public void Bind(TViewModel vm)
	{
		m_ViewModel = vm;
		if (m_ViewModel == null)
		{
			if (m_View != null)
			{
				Unbind();
			}
		}
		else
		{
			TryInstantiatePrefab();
			m_View.Bind(vm);
			vm.OnDispose += Unbind;
		}
	}

	private bool TryInstantiatePrefab()
	{
		if (m_View == null)
		{
			TView val = Load();
			m_View = UnityEngine.Object.Instantiate(val.gameObject, Target).GetComponent<TView>();
			if (CustomInitialize != null)
			{
				CustomInitialize(m_View);
			}
			if (m_View is IInitializable)
			{
				((IInitializable)m_View).Initialize();
			}
			return true;
		}
		return false;
	}

	private void Unbind()
	{
		if (m_ViewModel != null)
		{
			m_ViewModel.OnDispose -= Unbind;
		}
		m_ViewModel = null;
		if (!m_HoldAfterInstantiate)
		{
			UnityEngine.Object.Destroy(m_View.gameObject);
			m_View = null;
			ForceUnload();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
