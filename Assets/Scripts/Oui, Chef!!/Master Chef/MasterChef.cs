using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterChef : MonoBehaviour {
	
	public Order order;
	public int[] ingredients;
	public bool[] prepared;
	[SerializeField] private Kitchen kitchen;
	[SerializeField] private float speed = 5;
	[SerializeField] private Rigidbody2D rigidbody;
	[SerializeField] private Station targetStation = null;
	[SerializeField] private Vector3 heading;
	[SerializeField] private bool locked = false;
	[SerializeField] private float waitTime = 0.0f;


	void Start () {
		clearOrder();
	}
	
	void Update(){
		if(!kitchen.pause){
			if(!locked) act();
			else updateWaitTime();
		}
	}

	void FixedUpdate () {
		if(!kitchen.pause){
			if(!locked){
				move();
				updateTarget();
			}
		}
		else stop();
	}

	private void updateWaitTime(){
		if(waitTime > 0){
			waitTime -= Time.deltaTime;
		}
		else{
			waitTime = 0.0f;
			locked = false;
		}
	}

	public void wait(float time){
		locked = true;
		waitTime = time;
	}

	public void clearOrder(){
		order = null;
		ingredients = null;
		prepared = null;
	}

	private void move() {
		Vector3 movementVector = new Vector3(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"), 0.0f);

		if(movementVector != Vector3.zero){
			heading = movementVector.normalized;
			movementVector = movementVector * speed * Time.deltaTime; 
			//transform.Translate(movementVector);
			rigidbody.velocity = speed * movementVector.normalized;
			//transform.rotation = Quaternion.LookRotation(movementVector, Vector3.forward);
			transform.rotation =  Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(movementVector.normalized.y, movementVector.normalized.x) * 180/Mathf.PI);
		}
		else{
			stop();
		}
	}

	private void stop(){
		rigidbody.velocity = Vector3.zero;
	}
	private void updateTarget(){
		GameObject target = kitchen.updateTarget(transform.position, heading);

		if(target != null){
			targetStation = target.GetComponent<Station>();
		}
		else targetStation = null;
	}

	private void act(){
		bool interact = Input.GetButtonDown("Interact");

		if(interact && targetStation != null){
			stop();
			targetStation.interact();
		}
	}
}
