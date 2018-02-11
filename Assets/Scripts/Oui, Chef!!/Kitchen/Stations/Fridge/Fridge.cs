using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fridge : Station {

	[SerializeField] private GameObject itemPrefab;
	[SerializeField] private GameObject contents;
	[SerializeField] private GameObject itemsPanel;
	[SerializeField] private List<GameObject> items;
	[SerializeField] private string[] ingredients;
	void Start () {
		stationName = "Fridge";
		items = new List<GameObject>();
		itemsPanel.GetComponent<AutoGridLayout>().setRows(Mathf.RoundToInt(Mathf.Sqrt((int) Recipes.ingredients.Count)));

		ingredients = Recipes.getAllIngredientNames();
		Debug.Log("Ingredients: " + Functions.print(ingredients));
		foreach(string ingredient in ingredients){
			if(ingredient != "Count"){
				GameObject item =Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, itemsPanel.transform);
				item.GetComponent<FridgeItem>().setText(ingredient);
				items.Add(item);
			}
		}

		contents.SetActive(false);		
	}
	
	void Update () {
		
	}

	public void OnTriggerEnter2D(Collider2D col){
		Debug.Log("Fridge");
		if(col.gameObject.tag == "Master Chef"){
			kitchen.createTarget(this.gameObject, stationName, "Press", Controls.interact, "to open the fridge");
		}
	}

	public void OnTriggerExit2D(Collider2D col){
		Debug.Log("Exit fridge");
		if(col.gameObject.tag == "Master Chef"){
			kitchen.removeTarget(stationName);
		}
	}

	public override void interact(){
		if(!contents.activeSelf){
			Debug.Log("Open fridge");
		}
		else{
			System.Predicate<GameObject> itemSelected = x => x.GetComponent<FridgeItem>().selected;
			int[] selected = Functions.findAll(items.ToArray(), itemSelected);
			kitchen.setChefIngredients(selected);

			foreach(GameObject item in items){
				item.GetComponent<FridgeItem>().reset();
			}
		}
		
		toggleDisplayContents();
	}

	public override float employeeInteract(){return 5.0f;}

	private void toggleDisplayContents(){
		contents.SetActive(!contents.activeSelf);
	}
}
