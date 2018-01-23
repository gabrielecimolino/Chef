using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Employee : MonoBehaviour {

	public bool idle;
	public string employeeName = "Anon";
	[SerializeField] private GameObject verificationMenuPrefab;
	[SerializeField] private GameObject verificationMenu;
	[SerializeField] private Kitchen kitchen;
	[SerializeField] private float pathBuffer;
	[SerializeField] private float speed;
	private SimpleNeuralNetwork network;
	private Stack<int> destinations;
	private Stack<Node> path;
	private Order order;
	private int[] choices;
	[SerializeField] private float[] orderVector;
	[SerializeField] private float[] recipeVector;
	private const float vectorMin = -1.0f;

	void Start () {
		this.idle = true;
		this.verificationMenu = null;
		//this.employeeName = "Anon";
		//this.network = new SimpleNeuralNetwork((int) Menu.items.Count, new int[]{Mathf.FloorToInt(Mathf.Max((float) Menu.items.Count, (float) Recipes.ingredients.Count))}, (int) Recipes.ingredients.Count);
		this.network = new SimpleNeuralNetwork((int) Menu.items.Count, (int) Recipes.ingredients.Count);
		this.path = new Stack<Node>();
		this.destinations = new Stack<int>();
	}
	
	void Update () {
		if(verificationMenu != null){
			if(verificationMenu.GetComponent<VerificationMenu>().decided){
				if(verificationMenu.GetComponent<VerificationMenu>().accept){
					Destroy(verificationMenu);
					verificationMenu = null;
					sendOutOrder();
				}
				else{
					Destroy(verificationMenu);
					remakeOrder();
				}
			}
		}
	}

	void FixedUpdate(){
		move();
	}

	public void setKitchen(Kitchen kitchen){
		this.kitchen = kitchen;
	}

	public void setName(string employeeName){
		this.employeeName = employeeName;
	}

	public void setNextDestination(){
		if(destinations.Count > 0){
			int destination = destinations.Pop();

			//Debug.Log("Destination: " + destination.ToString());
			changePath(kitchen.getPath(transform.position, destination));
		}
		else{
			if(!idle && verificationMenu == null){
				presentDish();
			}
		}
	}

	public void changePath(Node[] path){
		this.path.Clear();
		for(int i = path.Length - 1; i >= 0; i--){
			this.path.Push(path[i]);
		}
	}

	private float[] getOrderVector(int[] order){
		float[] orderVector = Functions.initArray((int) Menu.items.Count, vectorMin);

		foreach(int item in order){
			orderVector[item] = 1.0f;
		}

		return orderVector;
	}

	public void giveOrder(Order order){
		this.order = order;
		idle = false;

		this.orderVector = getOrderVector(order.order);
		this.recipeVector = Recipes.getRecipeVector(order.order);

		network.feedForward(orderVector);

		int[] recipe = Functions.removeDuplicates(Functions.intComparer, order.recipe);
		this.choices = Functions.getNGreatestIndices(network.getOutputs(), recipe.Length);

		int[] destinations = kitchen.getPreparationLocations(choices);
		foreach(int destination in destinations){
			this.destinations.Push(destination);
		}

		setNextDestination();
	}

	private void move(){
		if(path.Count > 0){
			Node node = path.Peek();
			Vector3 difference = node.position - transform.position;
			float magnitude = Mathf.Abs((difference).magnitude);
			if(magnitude < pathBuffer){
				path.Pop();
			}

			transform.Translate(speed * Time.deltaTime * difference.normalized);
		}
		else{
			setNextDestination();
		}
	}

	private void presentDish(){
		verificationMenu = Instantiate(verificationMenuPrefab, Vector3.zero, Quaternion.identity, transform);
		verificationMenu.GetComponent<VerificationMenu>().setItems(order.order);
	}

	private void sendOutOrder(){
		kitchen.sendOutOrder(order, choices);
		idle = true;
	}

	private void remakeOrder(){
		network.train(recipeVector);

		giveOrder(order);
	}
}
