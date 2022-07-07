using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICAR1 : MonoBehaviour {

	public Transform[] Points;
	public float Speed = 0.0f, Distance = 0.0f;
	private int _currentPoint;

	void Update () {
		if (_currentPoint == Points.Length)_currentPoint = 0;
		float _currentDistance = Vector3.Distance (transform.position, Points [_currentPoint].position);
		if (_currentDistance <= Distance)_currentPoint++;
		Vector3 directionToPoint = Points [_currentPoint].position - transform.position;
		transform.up = Vector3.RotateTowards (transform.up, directionToPoint,Speed*Time.deltaTime,0.0f);
		transform.position = Vector3.MoveTowards (transform.position, Points [_currentPoint].position, Speed * Time.deltaTime);
	}
}
