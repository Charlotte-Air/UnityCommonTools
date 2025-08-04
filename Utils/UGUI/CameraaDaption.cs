using UnityEngine;

public class CameraaDaption : MonoBehaviour
{
    void Start ()
    {
	    var ManualWidth = GameConfig.GameInitWidth;
		var ManualHeight = GameConfig.GameInitHeight;
		int manualHeight;
		if (System.Convert.ToSingle(Screen.height) / Screen.width > System.Convert.ToSingle(ManualHeight) / ManualWidth)
			manualHeight = Mathf.RoundToInt(System.Convert.ToSingle(ManualWidth) / Screen.width * Screen.height);
		else
			manualHeight = (int)ManualHeight;
		var camera = GetComponent<Camera>();
		if (camera != null)
		{
			float scale = System.Convert.ToSingle(manualHeight / 1136f);
			camera.fieldOfView *= scale;
		}
	}
}