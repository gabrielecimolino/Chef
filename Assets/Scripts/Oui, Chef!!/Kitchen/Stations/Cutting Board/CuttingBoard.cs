using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingBoard : PrepStation {


	[SerializeField] private AudioSource cuttingSound;
	void Start () {
		stationName = "Cutting Board";
		prepTime = 2.0f;
		preparations = new int[]{ (int) Kitchen.preparations.Chop };
		buildIngredientClass();
	}
	
	void Update () {
		
	}

	public void OnTriggerEnter2D(Collider2D col){
		Debug.Log("Cutting board");
		if(col.gameObject.tag == "Master Chef"){
			kitchen.createTarget(this.gameObject, stationName, "Press", Controls.interact, "to chop");
		}
	}

	public void OnTriggerExit2D(Collider2D col){
		Debug.Log("Exit cutting board");
		if(col.gameObject.tag == "Master Chef"){
			if(kitchen.chefHasIngredients()) kitchen.removeTarget(stationName);
		}
	}

	public override void interact(){
		if(kitchen.chefHasIngredients()){
			cuttingSound.Play();
			kitchen.prepareFood(ingredientClass, prepTime);
		}
	}

	public override float employeeInteract(){
		cuttingSound.Play();
		return 10.0f;
	}
}
