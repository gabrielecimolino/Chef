using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order {

	public int[] order; // The integer values corresponding to the desired menu items
	public int[] recipe; // The integer values corresponding to the desired ingredients

	public Order(int[] order, int[] recipe){
		this.order = order;
		this.recipe = recipe;
	}
}
