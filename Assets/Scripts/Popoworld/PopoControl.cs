using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PopoControl : MonoBehaviour {
	private LineRenderer lineRenderer;
	private List<Vector3> touchPoints;
	private Transform player;
	private float startTime;
	private bool isFly;

	public float TOUCH_TOLERANCE = 0.1f;
	public float speed = 5.0f;

	void Start () {
		touchPoints = new List<Vector3>();
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.SetWidth(0.2f,0.2f);
		player = GameObject.FindGameObjectWithTag ("Player").transform;
		isFly = false;
	}

	void Update () {
		if (touchPoints.Count > 0) {
			// fixme: smooth the path
			lineRenderer.SetVertexCount(touchPoints.Count);
			for(int i = 0; i < touchPoints.Count; i++) {
				lineRenderer.SetPosition(i, touchPoints[i]);
			}
		} else
			lineRenderer.SetVertexCount(0);

		if (Input.GetMouseButtonDown(0) && isFly == false) {
			isFly = true;
			InvokeRepeating("AddTouchPoint", 0.02f, 0.02f);
			Invoke("FlySetup", 1f);
		} else if(Input.GetMouseButtonUp(0) && isFly == true){
			CancelInvoke ("AddTouchPoint");
		}
	}

	private void AddTouchPoint() {
		Vector3 screenPosition = Input.mousePosition;
		Vector3 worldPosition = Camera.main.ScreenToWorldPoint (screenPosition);
		worldPosition.z = -1;

		if (touchPoints.Count == 0) {
			touchPoints.Add (worldPosition);
		} else if (Vector3.Distance (worldPosition, touchPoints [touchPoints.Count - 1]) >= TOUCH_TOLERANCE) {
			touchPoints.Add (worldPosition);
		}
	}

	private void FlySetup() {
		// fixme: jump to the starting point(myPoint[0])
		player.position = touchPoints [0];

		player.gameObject.GetComponent<Rigidbody2D> ().gravityScale = 0;
		startTime = Time.time;
		InvokeRepeating("Fly", 0f, 0.02f);
	}

	private void Fly() {
		if (touchPoints.Count == 0) {
			CancelInvoke ();
			player.gameObject.GetComponent<Rigidbody2D>().gravityScale = 1;
			isFly = false;
			return;
		}

		float distRanInThePast = (Time.time - startTime) * speed;
		float distWithFirstPoint = 0;
		while (touchPoints.Count >= 2) {
			distWithFirstPoint = Vector3.Distance(touchPoints[0], touchPoints[1]);
			if (distRanInThePast < distWithFirstPoint) {
				break;
			}

			distRanInThePast -= distWithFirstPoint;
			touchPoints.RemoveAt(0);
		}

		if (touchPoints.Count == 1) {
			player.position = touchPoints[0];
			touchPoints.Clear();
		} else {
			player.position = Vector3.MoveTowards(touchPoints[0], touchPoints[1], distRanInThePast);
			touchPoints[0] = player.position;
		}

		startTime = Time.time;
	}
}