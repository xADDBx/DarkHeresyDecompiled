using System;
using System.Collections.Generic;
using Owlcat.UI;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class BoneListVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public List<BoneVM> Bones = new List<BoneVM>();

	public static Action<BoneVM> BoneMarkedForSeveringChanged;

	protected override void DisposeImplementation()
	{
	}
}
