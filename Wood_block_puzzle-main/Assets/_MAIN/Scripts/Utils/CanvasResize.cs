using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasResize : MonoBehaviour {
	Canvas m_canvas;
	UnityEngine.UI.CanvasScaler m_scaler;
	RectTransform m_rectTransform;
	RectTransform m_safeAreaTransform;

	float m_aspectRatio;
	float m_orthographicSize;
	float m_topOffsetY;
	float m_bottomOffsetY;

	void Awake() {
		Initialize();
	}
	
	void Initialize() {
		m_aspectRatio = (float)Screen.height / (float)Screen.width;
		m_orthographicSize = Camera.main.orthographicSize;

		m_canvas = GetComponent <Canvas> ();
		m_scaler = GetComponent <UnityEngine.UI.CanvasScaler> ();
		m_rectTransform = GetComponent <RectTransform> ();

		m_safeAreaTransform = m_rectTransform.Find("SafeArea") as RectTransform;

		ScaleCanvas();
		ApplySafeArea();
	}
	
	void ScaleCanvas() {
		if (m_aspectRatio > (1.8f)) { 
			// 18:9 ratio
			m_scaler.matchWidthOrHeight = 0;
		}
		else if (m_aspectRatio <= (1.5f)) {
			// Tablet ratio
			m_scaler.matchWidthOrHeight = 1;
		}
		else {
			// 16:9 or 16:10 ratio
			m_scaler.matchWidthOrHeight = 0;
		}
	}

	void ApplySafeArea() {
		Rect safeArea = Screen.safeArea;
		// Rect safeArea = new Rect(0, 132, 1125, 2172);

		var anchorMin = new Vector2(safeArea.position.x, (safeArea.position.y > 0) ? safeArea.position.y + 25 : safeArea.position.y);
		var anchorMax = anchorMin + safeArea.size;
		anchorMin.x /= m_canvas.pixelRect.width;
		anchorMin.y /= m_canvas.pixelRect.height;
		anchorMax.x /= m_canvas.pixelRect.width;
		anchorMax.y /= m_canvas.pixelRect.height;

		m_safeAreaTransform.anchorMin = anchorMin;
		m_safeAreaTransform.anchorMax = anchorMax;
	}
}
