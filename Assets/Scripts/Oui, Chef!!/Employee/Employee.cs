using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Employee : MonoBehaviour {

	public bool idle;
	public string employeeName = "Anon";
	[SerializeField] private AudioSource select;
	[SerializeField] private AudioSource deselect;
	[SerializeField] private AudioSource matt;
	[SerializeField] private GameObject shout;
	[SerializeField] private GameObject verificationMenuPrefab;
	[SerializeField] private GameObject verificationMenu;
	[SerializeField] private Kitchen kitchen;
	[SerializeField] private float pathBuffer;
	[SerializeField] private float speed;
	[SerializeField] private float shoutTime;
	[SerializeField] private Station targetStation;
	private SimpleNeuralNetwork network;
	private Stack<int> destinations;
	private Stack<Node> path;
	private Order order;
	[SerializeField] private int[] choices;
	[SerializeField] private float[] orderVector;
	[SerializeField] private float[] recipeVector;
	[SerializeField] private float shoutTimer;
	[SerializeField] private float waitTimer;
	[SerializeField] private bool waiting;
	private const float vectorMin = -1.0f;

	void Start () {
		this.idle = true;
		this.verificationMenu = null;
		//this.employeeName = "Anon";
		//this.network = new SimpleNeuralNetwork((int) Menu.items.Count, new int[]{Mathf.FloorToInt(Mathf.Max((float) Menu.items.Count, (float) Recipes.ingredients.Count))}, (int) Recipes.ingredients.Count);
		this.network = new SimpleNeuralNetwork((int) Menu.items.Count, (int) Recipes.ingredients.Count);
		this.path = new Stack<Node>();
		this.destinations = new Stack<int>();
		this.waitTimer = 0.0f;
		this.waiting = false;
		if(employeeName == "Matt") matt.Play();
	}
	
	void Update () {
		if(!waiting  && !kitchen.pause){
			updateShout();

			if(verificationMenu != null){
				if(verificationMenu.GetComponent<VerificationMenu>().decided){
					if(verificationMenu.GetComponent<VerificationMenu>().accept){
						select.Play();
						Destroy(verificationMenu);
						verificationMenu = null;
						sendOutOrder();
					}
					else{
						deselect.Play();
						shout.SetActive(true);
						shoutTimer = shoutTime;
						Destroy(verificationMenu);
						verificationMenu = null;
						remakeOrder();
					}
				}
			}
			else{
				if(destinations.Count == 0 && path.Count == 0 && !idle && verificationMenu == null){
					presentDish();
				}
			}

			move();
		}
		else{
			if(!kitchen.pause) updateWaiting();
		}
	}

	void FixedUpdate(){
	}

	private void updateWaiting(){
		if(waiting){
			if(waitTimer <= 0.0f){
				waitTimer = 0.0f;
				waiting = false;
				//setNextDestination();
			}
			else waitTimer -= Time.deltaTime;
		}
	}

	public void setKitchen(Kitchen kitchen){
		this.kitchen = kitchen;
	}

	public void setName(string employeeName){
		this.employeeName = employeeName;
	}

	public void setNextDestination(){
		Debug.Log("Setting destination");
		if(destinations.Count > 0){
			int destination = destinations.Pop();

			//Debug.Log("Destination: " + destination.ToString());
			changePath(kitchen.getPath(transform.position, destination));
			targetStation = kitchen.getStation(destination);
		}
		// else{
		// 	if(!idle && verificationMenu == null){
		// 		presentDish();
		// 	}
		// }
	}

	public void changePath(Node[] path){
		Debug.Log(Functions.print(Functions.map((x => x.position), path)));
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
		Debug.Log(Functions.print(destinations));
		foreach(int destination in destinations){
			this.destinations.Push(destination);
		}

		setNextDestination();
	}

	public void wait(float time){
		waiting = true;
		waitTimer = time;
	}

	private void move(){
		if(path.Count > 0){
			Node node = path.Peek();
			Vector3 difference = node.position - transform.position;
			float magnitude = Mathf.Abs((difference).magnitude);
			if(magnitude < pathBuffer){
				path.Pop();
				if(path.Count == 0){
					if(targetStation != null){
						Debug.Log("Interacting");
						wait(targetStation.employeeInteract());
						targetStation = null;
						setNextDestination();
					}
				}
			}

			transform.Translate(speed * Time.deltaTime * difference.normalized);
		}
		else{
			// if(destinations.Count > 0){
			// 	setNextDestination();
			// }
		}
	}

	private void updateShout(){
		if(shoutTimer > 0.0f){
			shoutTimer -= Time.deltaTime;
			if(shoutTimer <= 0) {
				shout.SetActive(false);
				shoutTimer = 0.0f;
			}
		}
		else{
			shoutTimer = 0.0f;
		}
	}

	private void presentDish(){
		verificationMenu = Instantiate(verificationMenuPrefab, Vector3.zero, Quaternion.identity, transform);
		verificationMenu.GetComponent<VerificationMenu>().setItems(order.order, choices);
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
