using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VerificationMenu : MonoBehaviour {
	public bool decided;
	public bool accept;
	[SerializeField] private GameObject orderItemPrefab;
	[SerializeField] private GameObject orderPanel;
	[SerializeField] private GameObject[] orderItems;
	[SerializeField] private GameObject ingredientPanel;
	[SerializeField] private GameObject[] ingredientItems;
	[SerializeField] private UIButton acceptButton;
	[SerializeField] private UIButton rejectButton;

	void Start () {
		orderItems = null;
		ingredientItems = null;
		decided = false;
		accept = false;
	}
	
	void Update () {
		bool accepted = acceptButton.getClicked();
		bool rejected = rejectButton.getClicked();
		if(accepted || rejected){
			decide(accepted);
		}
	}

	public void decide(bool decision){
		this.accept = decision;
		this.decided = true;
	}

	public void setItems(int[] order, int[] choices){
		decided = false;
		accept = false;
		string[] orderNames = Menu.getOrderItemNames(order);
		string[] ingredientNames = Recipes.getAllIngredientNames();
		Debug.Log("Ingredient Names: " + Functions.print(ingredientNames));

		orderItems = Functions.map((x => (Instantiate(orderItemPrefab, Vector3.zero, Quaternion.identity, orderPanel.transform))), orderNames);
		for(int i = 0; i < orderItems.Length; i++){
			orderItems[i].transform.Find("Text").GetComponent<Text>().text = orderNames[i];
		}

		ingredientItems = Functions.map((x => (Instantiate(orderItemPrefab, Vector3.zero, Quaternion.identity, ingredientPanel.transform))), choices);
		for(int i = 0; i < ingredientItems.Length; i++){
			ingredientItems[i].transform.Find("Text").GetComponent<Text>().text = ingredientNames[choices[i]];
		}
	}
}
