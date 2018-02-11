using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stove : PrepStation {

	[SerializeField] private AudioSource stoveSound;
	void Start () {
		stationName = "Stove";
		prepTime = 2.0f;
		preparations = new int[]{ (int) Kitchen.preparations.Boil, (int) Kitchen.preparations.Fry };
		buildIngredientClass();
	}
	
	void Update () {
		
	}

	public void OnTriggerEnter2D(Collider2D col){
		Debug.Log("Stove");
		if(col.gameObject.tag == "Master Chef"){
			if(kitchen.chefHasIngredients()) kitchen.createTarget(this.gameObject, stationName, "Press", Controls.interact, "to use the stove");
		}
	}

	public void OnTriggerExit2D(Collider2D col){
		Debug.Log("Exit stove");
		if(col.gameObject.tag == "Master Chef"){
			kitchen.removeTarget(stationName);
		}
	}

	public override void interact(){
		if(kitchen.chefHasIngredients()){
			stoveSound.Play();
			kitchen.prepareFood(ingredientClass, prepTime);
		}
	}

	public override float employeeInteract(){
		stoveSound.Play();
		return 10.0f;
	}
}
