﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Menu  {

	public enum items : int { 
		Gnocchi,
		Pizza,
		Frittata,
		Spaghetti, 
		Count
	};

	public static Dictionary<int,int> prices = new Dictionary<int, int>(){ 
		{(int) items.Gnocchi, 15 },
		{(int) items.Pizza, 20 },
		{(int) items.Frittata, 15 },
		{(int) items.Spaghetti, 10 }
	};

	public static int[] menu;

	public static void setMenu(int[] newMenu){
		menu = newMenu;
	}

	public static float[] createOrder(int[] desiredItems){
		if(desiredItems.Length > (int) items.Count) throw new System.ArgumentException("Menu::createOrder ~ number of desired items is greater than menu size\nDesired items: " + Functions.print(desiredItems));
		float[] order = Functions.initArray((int) items.Count, -1.0f);

		for(int i = 0; i < desiredItems.Length; i++){
			order[i] = 1.0f;
		}

		return order;
	}

	public static string[] getOrderItemNames(int[] order){
		return Functions.map((x => Enum.GetName(typeof(items), x)), order);
	}

	public static string getOrderItemName(int item){
		return Enum.GetName(typeof(items), item);
	}
}
