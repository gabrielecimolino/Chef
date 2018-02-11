using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mixer : PrepStation {
	[SerializeField] private AudioSource MixerSound;
	void Start () {
		stationName = "Mixer";
		prepTime = 2.0f;
		preparations = new int[]{ (int) Kitchen.preparations.Mix };
		buildIngredientClass();
	}
	
	void Update () {
		
	}

	public void OnTriggerEnter2D(Collider2D col){
		Debug.Log("Mixer");
		if(col.gameObject.tag == "Master Chef"){
			if(kitchen.chefHasIngredients()) kitchen.createTarget(this.gameObject, stationName, "Press", Controls.interact, "to use the mixer");
		}
	}

	public void OnTriggerExit2D(Collider2D col){
		Debug.Log("Exit Mixer");
		if(col.gameObject.tag == "Master Chef"){
			kitchen.removeTarget(stationName);
		}
	}

	public override void interact(){
		if(kitchen.chefHasIngredients()){
			MixerSound.Play();
			kitchen.prepareFood(ingredientClass, prepTime);
		}
	}

	public override float employeeInteract(){
		MixerSound.Play();
		return 10.0f;
	}
}
