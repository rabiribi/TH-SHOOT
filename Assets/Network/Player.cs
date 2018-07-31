using System.Collections;
using System.Collections.Generic;
using CCoder;
using CQueue;
using UnityEngine;

public class Player : MonoBehaviour
{
	public float moveSpeed = 10f;
	public Vector2 inputAxis;
	public Animator playerAnimator;
	public Rigidbody2D rig2D;
	public void Update()
	{
		
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
	public void FixedUpdate() {
		inputAxis = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
		//netWorkTest.client.Transmission(EnCoder.MoveMessage(inputAxis));
		ClQueue.ReadyForSend(EnCoder.MoveMessage(inputAxis.normalized,this.transform.position,moveSpeed));
	}
}
