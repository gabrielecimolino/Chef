using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recipes {

	private const float recipeNullValue = -1.0f;
	public enum ingredients : int { 
		Flour,
		Tomato,
		Egg,
		Potato,
		Mozzarella,
		Ham,
		Count
		};

	public static Dictionary<int,int[]> recipes = new Dictionary<int,int[]>(){
		{(int) Menu.items.Gnocchi, new int[] 			{ (int) ingredients.Potato, 	(int) ingredients.Egg, 		(int) ingredients.Tomato }},
		{(int) Menu.items.Pizza, new int[] 				{ (int) ingredients.Flour, 		(int) ingredients.Tomato, 	(int) ingredients.Mozzarella }},
		{(int) Menu.items.Frittata, new int[] 			{ (int) ingredients.Egg, 		(int) ingredients.Ham, 		(int) ingredients.Mozzarella }},
		{(int) Menu.items.Spaghetti, new int[] 			{ (int) ingredients.Egg, 		(int) ingredients.Flour, 	(int) ingredients.Tomato }}
	};

	public Recipes(){

	}

	public static float[] getRecipeVector(int[] order){
		if(order.Length > recipes.Count) throw new System.ArgumentException("Recipes::getRecipeVector ~ number of ordered items greater than number of known recipes\nOrder: " + Functions.print(order));
		float[] recipe = Functions.initArray((int) ingredients.Count, recipeNullValue);

		foreach(int item in order){
			int[] recipeIngredients = recipes[item];
			foreach(int ingredient in recipeIngredients){
				recipe[ingredient] = 1.0f;
			} 
		}

		return recipe;
	} 

	public static int[] getRecipe(int[] order){
		if(order.Length > recipes.Count) throw new System.ArgumentException("Recipes::getRecipe ~ number of ordered items greater than number of known recipes\nOrder: " + Functions.print(order));
		
		return Functions.foldl((Functions.cat), new int[]{}, (Functions.map((x => recipes[x]), order)));
	}

	public static string[] getIngredientNames(int[] order){
		string[] ingredientStrings = Enum.GetNames(typeof(ingredients));
		string[] ingredientNames = new string[]{};

		foreach(int item in order){
			int[] recipe = recipes[item];

			ingredientNames = Functions.cat(ingredientNames, Functions.map((x => ingredientStrings[x]), recipe));
		}

		return ingredientNames;
	}

	public static string getIngredientName(int ingredient){
		return Enum.GetName(typeof(ingredients), ingredient);
	}

	public static string[] getAllIngredientNames(){
		return Enum.GetNames(typeof(ingredients));
		// List<string> names = new List<string>();

		// for(int i = 0; i < (int) ingredients.Count; i++){

		// }
	}
}
