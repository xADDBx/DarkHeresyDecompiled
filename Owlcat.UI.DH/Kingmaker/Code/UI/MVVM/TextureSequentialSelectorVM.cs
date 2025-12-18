using System.Collections.Generic;
using Kingmaker.Blueprints.Base;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TextureSequentialSelectorVM : SequentialSelectorVM<TextureSequentialEntity>
{
	private readonly ReactiveProperty<Sprite> m_Value = new ReactiveProperty<Sprite>(null);

	public ReadOnlyReactiveProperty<Sprite> Value => m_Value;

	public TextureSequentialSelectorVM(bool cyclical = true)
		: base(cyclical)
	{
	}

	public TextureSequentialSelectorVM(List<TextureSequentialEntity> valueList, TextureSequentialEntity current = null, bool cyclical = true)
		: base(valueList, current, cyclical)
	{
	}

	protected override void SetCurrentEntity()
	{
		m_Value.Value = ValueList[base.CurrentIndex.CurrentValue].Texture;
	}

	protected override void SetSelectUIGender(Gender gender, int index)
	{
		if (base.Type == CharGenAppearancePageComponent.Gender && !UtilityNet.IsControlMainCharacter())
		{
			m_Value.Value = ValueList[index].Texture;
		}
	}
}
