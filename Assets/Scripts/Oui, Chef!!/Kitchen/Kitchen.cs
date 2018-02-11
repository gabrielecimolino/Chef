using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Kitchen : MonoBehaviour {

	public GameObject employeePrefab;
	public Employee[] employees;
	public MasterChef masterChef;
	public bool pause = false;

	[SerializeField] private int numberOfEmployees;
	[SerializeField] private PathNode[] nodes;
	[SerializeField] private OrderMenu ordersMenu;
	[SerializeField] private Prompt prompt;
	[SerializeField] private GameObject revenuePanel;
	[SerializeField] private ChefOrderMenu chefOrder;
	[SerializeField] private GameObject timer;
	[SerializeField] private bool[] employeesIdle;
	[SerializeField] private int[] idleEmployees; 
	[SerializeField] private Station fridge;
	[SerializeField] private Station stove;
	[SerializeField] private Station mixer;
	[SerializeField] private Station cuttingBoard;

	[SerializeField] private int fridgeIndex;
	[SerializeField] private int stoveIndex;
	[SerializeField] private int mixerIndex;
	[SerializeField] private int cuttingBoardIndex;
	[SerializeField] private int revenue;
	[SerializeField] private float time;
	private Queue<Order> orderQueue;
	private TargetingSystem targetingSystem;
	public enum preparations : int { Boil, Mix, Fry, Chop  };

	public Dictionary<int,int> ingredientClasses = new Dictionary<int,int>(){
		{(int) Recipes.ingredients.Tomato, (int) preparations.Chop},
		{(int) Recipes.ingredients.Egg, (int) preparations.Mix},
		{(int) Recipes.ingredients.Ham, (int) preparations.Fry},
		{(int) Recipes.ingredients.Flour, (int) preparations.Mix},
		{(int) Recipes.ingredients.Mozzarella, (int) preparations.Chop},
		{(int) Recipes.ingredients.Potato, (int) preparations.Boil}
	};

	void pathTest(){
		foreach(PathNode node in nodes){
			node.createNode();
		}
		foreach(PathNode node in nodes){
			node.addNeighbours();
		}
		Debug.Log("Running dijkstra");

		int start = Random.Range(0, nodes.Length -1);
		employees[0].transform.position = nodes[start].GetNode().position;
		int end = Random.Range(0, nodes.Length -1);
		Debug.Log("Path from " + start.ToString() + " to " + end.ToString());
		Node[] path = Dijkstra.dijkstra(Functions.map((x => x.GetNode()), nodes), start, end);
		Debug.Log(Functions.print(Functions.map((x => x.id), path)));

		employees[0].changePath(path);
	}
	void Start () {
		revenue = 0;
	
		foreach(PathNode node in nodes){
			node.createNode();
		}
		foreach(PathNode node in nodes){
			node.addNeighbours();
		}

		Menu.setMenu(new int[]{ 
			(int) Menu.items.Gnocchi,
			(int) Menu.items.Pizza,
			(int) Menu.items.Frittata,
			(int) Menu.items.Spaghetti
		});

		orderQueue = new Queue<Order>();
		targetingSystem = new TargetingSystem();

		this.employees = new Employee[numberOfEmployees];
		this.employees = Functions.map((x => (Instantiate(employeePrefab, Vector3.zero, Quaternion.identity, this.transform).GetComponent<Employee>())), employees);

		foreach(Employee employee in employees){
			employee.setKitchen(this);
			string name = Names.randomName();
			Debug.Log(name);
			employee.employeeName = name;
		}
		
		bool[] employeesIdle = Functions.map((x => x.idle), employees);
		this.employeesIdle = employeesIdle;
		this.idleEmployees = Functions.occurences(employeesIdle, true);

		//pathTest();	
	}
	
	void Update () {
		if(!pause){
			pause = Input.GetButtonDown("Pause");

			ordersMenu.checkClicked();
			updateEmployeeStatus();

			if(!ordersMenu.hasOrder){
				createOrder();
				ordersMenu.displayOrder(orderQueue.Dequeue(), idleEmployees, employees);
			}

			for(int i = 0; i < employees.Length; i++){
				if(employees[i].idle){
					//createOrder();
					//delegateOrder(i);
				}
			}

			updateTimer();
		}
		else{
			if(time > 0.0f){
				pause = !Input.GetButtonDown("Pause");
			}
		}
	}

	private void updateTimer(){
		time -= Time.deltaTime;
		if(time > 0.0f){
			string minutes = (Mathf.FloorToInt(time) / 60).ToString();
			string seconds = (Mathf.FloorToInt(time) % 60).ToString();
			if(seconds.Length == 1) seconds = "0" + seconds;
			timer.transform.Find("Text").GetComponent<Text>().text = "Time\n"
			+ minutes + ":" + seconds;
		}
		else{
			time = 0.0f;
			timer.transform.Find("Text").GetComponent<Text>().text = "Time\n0:00";
			pause = true;
		}
	}
	public int[] gatherIngredientClass(int preparation){
		List<int> ingredients = new List<int>();
		
		for(int i = 0; i < (int) Recipes.ingredients.Count; i++){
			if((int) ingredientClasses[i] == preparation) ingredients.Add(i);
		}

		return ingredients.ToArray();
	}

	public void createTarget(GameObject target, string name, string left, Sprite image = null, string right = null){
		targetingSystem.addTarget(name, target, target.transform.position);
		prompt.addMessage(left, image, right, name);
		prompt.setMessage(name);
	}

	public void removeTarget(string name){
		targetingSystem.removeTarget(name);
		prompt.removeMessage(name);
	}

	public GameObject updateTarget(Vector3 position, Vector3 heading){
		Target target = targetingSystem.getTarget(position, heading);

		if(target != null){
			prompt.setMessage(target.targetName);
			return target.target;
		}
		else return null;
	}

	public bool setChefOrder(Order order){
		if(chefOrder.setOrder(order)){
			masterChef.order = order;
			return true;
		}
		else return false;
	}

	public void setChefIngredients(int[] ingredients){
		masterChef.ingredients = ingredients;
		masterChef.prepared = Functions.initArray(ingredients.Length, false);
		chefOrder.setIngredients(ingredients);
	}

	public bool chefOrderReady(){
		return (masterChef.prepared != null && masterChef.order != null) ? Functions.all(Functions.identity, masterChef.prepared) : false;
	}

	public bool sendOutChefOrder(){
		if(chefOrderReady()){
			sendOutOrder(masterChef.order, masterChef.ingredients);
			masterChef.clearOrder();
			chefOrder.clearOrder();
			return true;
		}
		else return false;
	}

	public bool chefHasIngredients(){
		return masterChef.ingredients != null;
	}

	public void prepareFood(int[] ingredientClass, float prepTime){
		if(chefHasIngredients()){
			masterChef.wait(prepTime);
			List<int> preparedItems = new List<int>();

			for(int i = 0; i < masterChef.ingredients.Length; i++){
				if(Functions.contains(masterChef.ingredients[i], ingredientClass)){
					masterChef.prepared[i] = true;
					preparedItems.Add(masterChef.ingredients[i]);
				}
			}

			chefOrder.ingredientsPrepared(preparedItems.ToArray());
		}
	}

	private void updateEmployeeStatus(){
		bool[] idle = Functions.map((x => x.idle), employees);

		if(ordersMenu.hasOrder){
			for(int i = 0; i < employeesIdle.Length; i++){
				if(!this.employeesIdle[i] && idle[i]) ordersMenu.addIdleEmployee(i, employees[i]);
			}
		}

		
		idleEmployees = Functions.occurences(idle, true);
		employeesIdle = idle;
	}

	public void createOrder(){
		// Orders have between 1 and sqrt(|menu|) items
		int[] order = Functions.initArray(Random.Range(1, Mathf.RoundToInt(Mathf.Sqrt(Menu.menu.Length)) + 1), 0);
		order = Functions.map((x => Random.Range(x, Menu.menu.Length - 1)), order);
		
		Order newOrder = new Order(order, Recipes.getRecipe(order));
		orderQueue.Enqueue(newOrder);
	}

	public void delegateOrder(int employee){
		Order order = orderQueue.Dequeue();

		employees[employee].giveOrder(order);
	}

	public void sendOutOrder(Order order, int[] choices){
		int score = Functions.foldl(((x,y) => (Functions.contains(y, order.recipe) ? x + 1 : x)), 0, choices);
		float accuracy = (float) score / choices.Length;
		int[] bill = Functions.map((x => Menu.prices[x]), order.order);
		updateRevenue(Mathf.FloorToInt(accuracy * Functions.sum(bill)));
		Debug.Log("Recipe: " + Functions.print(order.recipe));
		Debug.Log("Choices: " + Functions.print(choices));
		Debug.Log(accuracy);
	}

	private void updateRevenue(int d){
		revenue += d;
		revenuePanel.transform.Find("Text").GetComponent<Text>().text = "Cheese\n" + revenue.ToString() + "$";
	}

	public int[] getPreparationLocations(int[] choices){
		int[] locations = new int[choices.Length];
		locations[locations.Length - 1] = fridgeIndex;

		choices = Functions.shuffle(choices);
		for(int i = 0; i < choices.Length - 1; i++){
			switch((int) ingredientClasses[choices[i]]){
				case ((int) preparations.Boil):
					Debug.Log("Go to stove");
					locations[i] = stoveIndex;
					break;
				case ((int) preparations.Chop):
					Debug.Log("Go to board");
					locations[i] = cuttingBoardIndex;
					break;
				case ((int) preparations.Fry):
					Debug.Log("Go to stove");
					locations[i] = stoveIndex;
					break;
				case ((int) preparations.Mix):
					Debug.Log("Go to mixer");
					locations[i] = mixerIndex;
					break;
				default:
					Debug.Log("No preparation for item" + ((int) ingredientClasses[choices[i]]).ToString());
					break;
			}
		}

		return locations;
	}

	public Node[] getPath(Vector3 position, int destination){
		// Starts at closest node
		int start = Functions.minIndex(Functions.map((x => (x.GetNode().position - position).magnitude), nodes));
		return Dijkstra.dijkstra(Functions.map((x => x.GetNode()), nodes), start, destination);
	}

	public Station getStation(int station){
		if(station == fridgeIndex) return fridge;
		if(station == stoveIndex) return stove;
		if(station == mixerIndex) return mixer;
		if(station == cuttingBoardIndex) return cuttingBoard;
		return null;
	}
}
