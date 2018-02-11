using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PrepStation : Station {

	[SerializeField] protected int[] preparations;
	[SerializeField] protected int[] ingredientClass;
	[SerializeField] protected float prepTime;

	protected virtual void buildIngredientClass(){
		ingredientClass = new int[]{};

		foreach(int preparation in preparations){
			ingredientClass = Functions.cat(ingredientClass, kitchen.gatherIngredientClass(preparation));
		}
	}
}
