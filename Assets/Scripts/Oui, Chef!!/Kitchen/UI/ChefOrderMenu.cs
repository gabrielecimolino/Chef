using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChefOrderMenu : MonoBehaviour {

	public GameObject itemPrefab;
	public GameObject orderPanel;
	public GameObject ingredientPanel;
	[SerializeField] private List<GameObject> orderItems;
	[SerializeField] private List<GameObject> ingredientItems;
	[SerializeField] private int[] ingredients;
	[SerializeField] private bool hasOrder;
	void Start () { 
		//setOrder(new int[]{1,3});
		//setIngredients(new int[]{2,4,5});
	}
	
	// void Update () {
		
	// }

	public void clearOrder(){
		foreach(GameObject item in orderItems){
			Destroy(item);
		}

		foreach(GameObject item in ingredientItems){
			Destroy(item);
		}

		hasOrder = false;
	}

	public bool setOrder(Order order){
		if(hasOrder) return false;
		else{
			foreach(GameObject item in orderItems){
				Destroy(item);
			}

			orderItems = new List<GameObject>();

			foreach(int item in order.order){
				GameObject orderItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, orderPanel.transform);
				orderItem.transform.Find("Text").GetComponent<Text>().text = Menu.getOrderItemName(item);
				orderItems.Add(orderItem);
			}

			hasOrder = true;
		}

		return true;
	}

	public void setIngredients(int[] ingredients){
		this.ingredients = ingredients;
		foreach(GameObject item in ingredientItems){
			Destroy(item);
		}

		ingredientItems = new List<GameObject>();

		foreach(int item in ingredients){
			GameObject ingredientItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, ingredientPanel.transform);
			ingredientItem.transform.Find("Text").GetComponent<Text>().text = Recipes.getIngredientName(item);
			ingredientItems.Add(ingredientItem);
		}

		ingredientPanel.GetComponent<AutoGridLayout>().setRows(ingredients.Length / 3);
	}

	public void ingredientsPrepared(int[] ingredients){
		foreach(int ingredient in ingredients){
			int index = Functions.find(this.ingredients, ingredient);
			ingredientItems[index].transform.Find("Check").gameObject.SetActive(true);
		}
	}
}
