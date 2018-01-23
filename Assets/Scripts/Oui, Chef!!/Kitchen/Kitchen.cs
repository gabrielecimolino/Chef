using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kitchen : MonoBehaviour {

	public GameObject employeePrefab;
	public Employee[] employees;
	[SerializeField] private int numberOfEmployees;
	[SerializeField] private PathNode[] nodes;
	[SerializeField] private OrderMenu ordersMenu;
	[SerializeField] private bool[] employeesIdle;
	[SerializeField] private int[] idleEmployees; 
	[SerializeField] private int stove;
	[SerializeField] private int counter;
	[SerializeField] private int cuttingBoard;
	private Queue<Order> orderQueue;

	private enum preparations : int { Boil, Mix, Fry, Chop  };

	private Dictionary<int,int> ingredientClasses = new Dictionary<int,int>(){
		{(int) Recipes.ingredients.Tomato, (int) preparations.Boil},
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
		Debug.Log(Functions.search(Functions.intComparer, 7, new int[]{}));
		Debug.Log(Functions.search(Functions.intComparer, 7, new int[]{ 1,2,3,4,5,6,7,8,9 }));
		Debug.Log(Functions.search(Functions.intComparer, 7, new int[]{ 1,2,3,4,5,6 }));
		Debug.Log(Functions.print(Functions.removeDuplicates(Functions.intComparer, new int[]{ 3,2,3,4,4,5,6,2,4 })));
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
		Debug.Log("Recipe: " + Functions.print(order.recipe));
		Debug.Log("Choices: " + Functions.print(choices));
		Debug.Log(accuracy);
	}

	public int[] getPreparationLocations(int[] choices){
		int[] locations = new int[choices.Length];
		
		for(int i = 0; i < choices.Length; i++){
			switch((int) ingredientClasses[choices[i]]){
				case ((int) preparations.Boil):
					locations[i] = stove;
					break;
				case ((int) preparations.Chop):
					locations[i] = cuttingBoard;
					break;
				case ((int) preparations.Fry):
					locations[i] = stove;
					break;
				case ((int) preparations.Mix):
					locations[i] = counter;
					break;
				default:
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
}
