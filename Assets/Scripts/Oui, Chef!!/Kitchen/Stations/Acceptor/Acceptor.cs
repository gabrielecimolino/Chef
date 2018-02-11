using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Acceptor : Station {

	[SerializeField] AudioSource till;

	void Start () {
		
	}
	
	void Update () {
		
	}

	public void OnTriggerEnter2D(Collider2D col){
		Debug.Log("Acceptor");
		if(col.gameObject.tag == "Master Chef"){
			if(kitchen.chefOrderReady()) kitchen.createTarget(this.gameObject, stationName, "Press", Controls.interact, "to send out the order");
		}
	}

	public void OnTriggerExit2D(Collider2D col){
		Debug.Log("Exit Acceptor");
		if(col.gameObject.tag == "Master Chef"){
			kitchen.removeTarget(stationName);
		}
	}
	public override void interact(){
		if(kitchen.chefOrderReady()){
			till.Play();
			kitchen.sendOutChefOrder();
		}
	}

	public override float employeeInteract(){return 1.0f;}
}
