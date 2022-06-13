/* **************************************************************************
 * FPS COUNTER
 * **************************************************************************
 * Written by: Annop "Nargus" Prapasapong
 * Created: 7 June 2012
 * *************************************************************************/
 
using UnityEngine;
using System.Collections;
 
/* **************************************************************************
 * CLASS: FPS COUNTER
 * *************************************************************************/ 
public class FPSCounter : MonoBehaviour {
	/* Public Variables */
	public float frequency = 0.5f;
    public TMPro.TextMeshProUGUI counterText;
 
	/* **********************************************************************
	 * PROPERTIES
	 * *********************************************************************/
	public int FramesPerSec { get; protected set; }
 
	/* **********************************************************************
	 * EVENT HANDLERS
	 * *********************************************************************/
	/*
	 * EVENT: Start
	 */
	private void Start() {
		StartCoroutine(FPS());
	}
 
	/*
	 * EVENT: FPS
	 */
	private IEnumerator FPS() {
		for(;;){
			// Capture frame-per-second
			int lastFrameCount = Time.frameCount;
			float lastTime = Time.realtimeSinceStartup;
			yield return new WaitForSeconds(frequency);
			float timeSpan = Time.realtimeSinceStartup - lastTime;
			int frameCount = Time.frameCount - lastFrameCount;
 
			// Display it
			FramesPerSec = Mathf.RoundToInt(frameCount / timeSpan);
			counterText.text = FramesPerSec.ToString() + " fps";
            
            if (FramesPerSec <= 30)
                counterText.color = Color.red;
            else if (FramesPerSec <= 50)
                counterText.color = Color.yellow;
            else if (FramesPerSec <= 70)
                counterText.color = Color.green;
            else 
                counterText.color = Color.white;
		}
	}
}