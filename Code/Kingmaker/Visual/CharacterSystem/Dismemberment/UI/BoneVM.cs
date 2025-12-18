using System;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class BoneVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public Transform Bone;

	public ReactiveProperty<bool> IsMarkedForSevering = new ReactiveProperty<bool>(value: false);

	public BoneVM(Transform bone)
	{
		Bone = bone;
	}

	protected override void DisposeImplementation()
	{
	}

	internal void OnSeveringChanged(bool value)
	{
		IsMarkedForSevering.Value = value;
		BoneListVM.BoneMarkedForSeveringChanged?.Invoke(this);
	}
}
