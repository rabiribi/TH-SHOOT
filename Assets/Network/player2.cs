using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player2 : MonoBehaviour {
	public static float moveSpeed = 10f;
	public static Vector2 inputAxis;
	public static Vector3 Position;
	public Animator playerAnimator;
	public Rigidbody2D rig2D;
	public void Start() {
		Position = transform.position;
	}
	public void Update()
	{
		SetPosition(Position);
		if (inputAxis.magnitude == 0)
		{
			rig2D.velocity = Vector2.zero;
			playerAnimator.SetFloat("inputX", inputAxis.x);
			playerAnimator.SetFloat("inputY", inputAxis.y);
		}
		else
		{
			rig2D.velocity = moveSpeed * inputAxis.normalized;
			//playerAnimator.SetFloat("inputX", inputAxis.x);
			//playerAnimator.SetFloat("inputY", inputAxis.y);
			playerAnimator.SetFloat("lastInputX", inputAxis.x);
			playerAnimator.SetFloat("lastInputY", inputAxis.y);
		}
	}
	private void SetPosition (Vector2 position) {
		this.GetComponent<Transform>().position = new Vector3(position.x,position.y,transform.position.z);
	}
}
