using UnityEngine;
using System.Collections;

public class RotateCamera : MonoBehaviour {
    public float speed = 1.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        float dt = Time.deltaTime;
        //Vector3 cur_rot = transform.rotation.eulerAngles;
        //Debug.Log(cur_rot.y);
        transform.Rotate(new Vector3 (0.0f, speed * dt, 0.0f) );
    }
}
