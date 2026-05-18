using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class NewStraightLine : MonoBehaviour
{
	[SerializeField]
	private Image m_LinePart;

	[SerializeField]
	private float m_Epsilon = 0.1f;

	[SerializeField]
	private float m_LineExtension = 2f;

	private readonly List<RectTransform> m_LineObjects = new List<RectTransform>();

	private List<Vector2> m_PathPoints = new List<Vector2>();

	[SerializeField]
	private RectTransform m_CluesContainer;

	[SerializeField]
	private bool m_DoRemovePoints = true;

	[field: SerializeField]
	public LineDirectionData StartPoint { get; private set; }

	[field: SerializeField]
	public LineDirectionData EndPoint { get; private set; }

	private void OnValidate()
	{
		if (StartPoint.Dot != null && EndPoint.Dot != null)
		{
			CalculatePath();
		}
	}

	private void LateUpdate()
	{
		if (StartPoint != null && EndPoint != null)
		{
			CalculatePath();
			DrawUILines();
		}
	}

	public void Initialize(LineDirectionData fromDirection, LineDirectionData toDirection, RectTransform cluesContainer, Color? color = null)
	{
		StartPoint = fromDirection;
		EndPoint = toDirection;
		m_CluesContainer = cluesContainer;
		if (color.HasValue)
		{
			m_LinePart.color = color.Value;
		}
		CalculatePath();
	}

	public void SetColor(Color color)
	{
		foreach (RectTransform lineObject in m_LineObjects)
		{
			lineObject.EnsureComponent<Image>().color = color;
		}
	}

	public void SetDirection(int id, LineDirectionData direction)
	{
		if (id == 0)
		{
			StartPoint = direction;
		}
		else
		{
			EndPoint = direction;
		}
	}

	public void SetDirection(int id, LineDirection direction)
	{
		if (id == 0)
		{
			StartPoint.Direction = direction;
		}
		else
		{
			EndPoint.Direction = direction;
		}
	}

	public void SetLength(int id, float length)
	{
		if (id == 0)
		{
			StartPoint.Length = length;
		}
		else
		{
			EndPoint.Length = length;
		}
	}

	public void UpdateDot(int id, RectTransform dot)
	{
		if (id == 0)
		{
			StartPoint.Dot = dot;
		}
		else
		{
			EndPoint.Dot = dot;
		}
	}

	private void CalculatePath()
	{
		m_PathPoints.Clear();
		if (StartPoint.Dot == null || EndPoint.Dot == null)
		{
			return;
		}
		Vector2 vector = m_CluesContainer.InverseTransformPoint(StartPoint.Dot.position);
		Vector2 vector2 = m_CluesContainer.InverseTransformPoint(EndPoint.Dot.position);
		Vector2 vector3 = StartPoint.Direction.GetDirectionVector();
		Vector2 vector4 = EndPoint.Direction.GetDirectionVector() * -1f;
		m_PathPoints.Add(vector);
		Vector2 vector5 = vector + vector3 * StartPoint.Length;
		Vector2 vector6 = vector2 + vector4 * EndPoint.Length;
		bool flag = Mathf.Approximately(vector3.y, 0f);
		bool flag2 = Mathf.Approximately(vector4.y, 0f);
		m_PathPoints.Add(vector5);
		Vector2 vector7;
		bool flag3;
		float x;
		if ((flag && !flag2) || (!flag && flag2))
		{
			vector7 = (flag ? new Vector2(vector6.x, vector5.y) : new Vector2(vector5.x, vector6.y));
			Vector2 normalized = (vector7 - vector5).normalized;
			Vector2 normalized2 = (vector5 - vector).normalized;
			float a = Vector2.Dot(normalized, normalized2);
			flag3 = true;
			if (flag)
			{
				if (!Mathf.Approximately(vector7.x, vector6.x) || !Mathf.Approximately(a, 1f))
				{
					x = vector3.x;
					if (!(x > 0f))
					{
						if (x < 0f && vector7.x > vector5.x)
						{
							goto IL_01f8;
						}
					}
					else if (vector7.x < vector5.x)
					{
						goto IL_01f8;
					}
					goto IL_01fb;
				}
			}
			else if (!Mathf.Approximately(vector7.y, vector6.y) || !Mathf.Approximately(a, 1f))
			{
				x = vector3.y;
				if (!(x > 0f))
				{
					if (x < 0f && vector7.y > vector5.y)
					{
						goto IL_02ae;
					}
				}
				else if (vector7.y < vector5.y)
				{
					goto IL_02ae;
				}
				goto IL_02b1;
			}
			goto IL_02f2;
		}
		if ((!flag || !Mathf.Approximately(vector5.y, vector6.y)) && (flag || !Mathf.Approximately(vector5.x, vector6.x)))
		{
			if (flag)
			{
				Vector2 item = new Vector2(vector5.x, vector6.y);
				m_PathPoints.Add(item);
			}
			else
			{
				Vector2 item2 = new Vector2(vector6.x, vector5.y);
				m_PathPoints.Add(item2);
			}
		}
		goto IL_03d3;
		IL_01fb:
		x = vector4.y;
		if (!(x > 0f))
		{
			if (x < 0f && vector7.y > vector6.y)
			{
				goto IL_0242;
			}
		}
		else if (vector7.y < vector6.y)
		{
			goto IL_0242;
		}
		goto IL_02f2;
		IL_02f2:
		if (flag3)
		{
			m_PathPoints.Add(vector7);
		}
		else if (flag)
		{
			Vector2 item3 = new Vector2(vector5.x, vector6.y);
			m_PathPoints.Add(item3);
		}
		else
		{
			Vector2 item4 = new Vector2(vector6.x, vector5.y);
			m_PathPoints.Add(item4);
		}
		goto IL_03d3;
		IL_02b1:
		x = vector4.x;
		if (!(x > 0f))
		{
			if (x < 0f && vector7.x > vector6.x)
			{
				goto IL_02ef;
			}
		}
		else if (vector7.x < vector6.x)
		{
			goto IL_02ef;
		}
		goto IL_02f2;
		IL_02ef:
		flag3 = false;
		goto IL_02f2;
		IL_02ae:
		flag3 = false;
		goto IL_02b1;
		IL_0242:
		flag3 = false;
		goto IL_02f2;
		IL_03d3:
		List<Vector2> pathPoints = m_PathPoints;
		if ((pathPoints[pathPoints.Count - 1] - vector6).sqrMagnitude > m_Epsilon)
		{
			m_PathPoints.Add(vector6);
		}
		m_PathPoints.Add(vector2);
		if (m_DoRemovePoints)
		{
			RemoveRedundantPoints();
		}
		return;
		IL_01f8:
		flag3 = false;
		goto IL_01fb;
	}

	private void RemoveRedundantPoints()
	{
		if (m_PathPoints.Count < 3)
		{
			return;
		}
		List<Vector2> pathPoints = m_PathPoints;
		Vector2 vector = pathPoints[pathPoints.Count - 1];
		List<Vector2> pathPoints2 = m_PathPoints;
		Vector2 vector2 = pathPoints2[pathPoints2.Count - 2];
		List<Vector2> pathPoints3 = m_PathPoints;
		Vector2 vector3 = pathPoints3[pathPoints3.Count - 3];
		Vector2 normalized = (vector - vector2).normalized;
		Vector2 normalized2 = (vector2 - vector3).normalized;
		if (!Mathf.Approximately(Vector2.Dot(normalized, normalized2), 1f))
		{
			if (Mathf.Approximately(vector3.y, vector2.y) && Mathf.Approximately(vector2.y, vector.y))
			{
				float num = ((m_CluesContainer.InverseTransformPoint(StartPoint.Dot.position) + StartPoint.Direction.GetDirectionVector() * StartPoint.Length).y - (m_CluesContainer.InverseTransformPoint(EndPoint.Dot.position) + EndPoint.Direction.GetDirectionVector() * EndPoint.Length).y) * 0.5f;
				Vector2 item = vector2;
				vector2.y += num;
				vector3.y += num;
				List<Vector2> pathPoints4 = m_PathPoints;
				pathPoints4[pathPoints4.Count - 2] = vector2;
				List<Vector2> pathPoints5 = m_PathPoints;
				pathPoints5[pathPoints5.Count - 3] = vector3;
				m_PathPoints.Insert(m_PathPoints.Count - 1, item);
			}
			else if (Mathf.Approximately(vector3.x, vector2.x) && Mathf.Approximately(vector2.x, vector.x))
			{
				float num2 = ((m_CluesContainer.InverseTransformPoint(StartPoint.Dot.position) + StartPoint.Direction.GetDirectionVector() * StartPoint.Length).x - (m_CluesContainer.InverseTransformPoint(EndPoint.Dot.position) + EndPoint.Direction.GetDirectionVector() * EndPoint.Length).x) * 0.5f;
				Vector2 item2 = vector2;
				vector2.x += num2;
				vector3.x += num2;
				List<Vector2> pathPoints6 = m_PathPoints;
				pathPoints6[pathPoints6.Count - 2] = vector2;
				List<Vector2> pathPoints7 = m_PathPoints;
				pathPoints7[pathPoints7.Count - 3] = vector3;
				m_PathPoints.Insert(m_PathPoints.Count - 1, item2);
			}
		}
		for (int num3 = m_PathPoints.Count - 2; num3 > 0; num3--)
		{
			if (num3 - 1 >= 0 && num3 + 1 < m_PathPoints.Count)
			{
				Vector2 a = m_PathPoints[num3 - 1];
				Vector2 b = m_PathPoints[num3];
				Vector2 c = m_PathPoints[num3 + 1];
				if (ArePointsCollinear(a, b, c))
				{
					m_PathPoints.RemoveAt(num3);
					num3--;
				}
			}
		}
	}

	private bool ArePointsCollinear(Vector2 a, Vector2 b, Vector2 c)
	{
		if (Mathf.Abs(a.y - b.y) < m_Epsilon && Mathf.Abs(b.y - c.y) < m_Epsilon)
		{
			return true;
		}
		if (Mathf.Abs(a.x - b.x) < m_Epsilon && Mathf.Abs(b.x - c.x) < m_Epsilon)
		{
			return true;
		}
		return false;
	}

	private void DrawUILines()
	{
		if (m_PathPoints.Count >= 2)
		{
			for (int i = m_LineObjects.Count; i < m_PathPoints.Count - 1; i++)
			{
				GameObject gameObject = Object.Instantiate(m_LinePart.gameObject, base.transform, worldPositionStays: false);
				m_LineObjects.Add(gameObject.GetComponent<RectTransform>());
			}
			for (int j = 0; j < m_PathPoints.Count - 1; j++)
			{
				Vector2 startPoint = m_PathPoints[j];
				Vector2 endPoint = m_PathPoints[j + 1];
				m_LineObjects[j].gameObject.SetActive(value: true);
				ConfigureRectTransformLine(m_LineObjects[j], startPoint, endPoint);
			}
			for (int k = m_PathPoints.Count - 1; k < m_LineObjects.Count; k++)
			{
				m_LineObjects[k].gameObject.SetActive(value: false);
			}
		}
	}

	private void ConfigureRectTransformLine(RectTransform rectTransform, Vector2 startPoint, Vector2 endPoint)
	{
		Vector2 normalized = (endPoint - startPoint).normalized;
		Vector2 vector = startPoint - normalized * m_LineExtension;
		Vector2 vector2 = endPoint + normalized * m_LineExtension;
		Vector2 anchoredPosition = (vector + vector2) / 2f;
		rectTransform.anchoredPosition = anchoredPosition;
		float x = Vector2.Distance(vector, vector2);
		float z = Mathf.Atan2(vector2.y - vector.y, vector2.x - vector.x) * 57.29578f;
		rectTransform.sizeDelta = new Vector2(x, rectTransform.sizeDelta.y);
		rectTransform.rotation = Quaternion.Euler(0f, 0f, z);
	}
}
