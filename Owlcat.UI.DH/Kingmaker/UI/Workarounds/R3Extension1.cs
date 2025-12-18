using System;
using R3;
using TMPro;

namespace Kingmaker.UI.Workarounds;

[Obsolete]
public static class R3Extension1
{
	public static Observable<string> OnEndEditAsObservable(this TMP_InputField inputField)
	{
		return inputField.onEndEdit.AsObservable();
	}

	public static Observable<string> OnValueChangedAsObservable(this TMP_InputField inputField)
	{
		return Observable.Create(inputField, delegate(Observer<string> observer, TMP_InputField i)
		{
			observer.OnNext(i.text);
			return i.onValueChanged.AsObservable().Subscribe(observer);
		});
	}
}
