using UnityEngine;
using System.Collections;

public class Touch : MonoBehaviour {
    public Camera cam;
    private static Vector2 pos;
    public float speed = 1.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.touchCount > 0)
        {
            UnityEngine.Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // タッチ開始
                pos = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                // タッチ移動
                Vector2 cpos = touch.position;
                Vector2 dpos = cpos - pos;
                //Vector3 cur_rot = cam.transform.rotation.eulerAngles;
                cam.transform.Rotate(new Vector3(speed*dpos.y, -speed*dpos.x, 0.0f));
                pos = cpos;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                // タッチ終了
            }
        }
    }
}
