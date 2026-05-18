using System;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Data;

[Serializable]
public struct TextStyle
{
	[SerializeField]
	private TMP_StyleSheet m_StyleSheet;

	[SerializeField]
	private string m_StyleName;

	public TMP_StyleSheet StyleSheet => m_StyleSheet ?? TMP_Settings.defaultStyleSheet;

	public TMP_Style Style => StyleSheet.GetStyle(m_StyleName ?? "Normal");
}
