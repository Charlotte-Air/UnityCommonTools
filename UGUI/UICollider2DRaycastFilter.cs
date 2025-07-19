 using UnityEngine;
 using UnityEngine.UI;
  
namespace Soda
{
	[RequireComponent (typeof (RectTransform), typeof (Collider2D))]
	public class UICollider2DRaycastFilter : MonoBehaviour, ICanvasRaycastFilter 
	{
		private Collider2D m_collider;
		private RectTransform m_rectTransform;

		private void Awake () 
		{
			m_collider = GetComponent<Collider2D>();
			m_rectTransform = GetComponent<RectTransform>();
		}

		public bool IsRaycastLocationValid (Vector2 screenPos, Camera eventCamera)
		{
			Vector3 wpos = Vector3.zero;

			bool inside = RectTransformUtility.ScreenPointToWorldPointInRectangle (m_rectTransform, screenPos, eventCamera, out wpos);

			if (inside)
			{
				inside = m_collider.OverlapPoint (wpos);
			}

			return inside;
		}
	}
}