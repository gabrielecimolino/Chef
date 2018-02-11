using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderMenu : MonoBehaviour {

	public bool hasOrder = false;
	public Kitchen kitchen;
	[SerializeField] private GameObject orderPrefab;
	[SerializeField] private GameObject orderItemPrefab;
	[SerializeField] private GameObject idleEmployeePrefab;
	[SerializeField] private GameObject orderPanel;
	[SerializeField] private GameObject employeeGrid;
	[SerializeField] private GameObject currentOrder;
	[SerializeField] private GameObject[] orderItems;
	[SerializeField] private GameObject[] employeeItems;
	[SerializeField] private UIButton takeButton;
	[SerializeField] private Employee[] employees;
	[SerializeField] private int[] idleEmployees;
	[SerializeField] private bool take;
	private Order order;

	private const int maxColumns = 4;
	
	void Start () {
		
	}

	public void checkClicked(){
		if(hasOrder){
			take = takeButton.getClicked();
			if(take){
				if(kitchen.setChefOrder(order)) clearOrder();
			}
			else{
				bool[] clicked = Functions.map((x => x.GetComponent<UIButton>().getClicked()), employeeItems);

				if(Functions.any(clicked)){
					delegateOrder(idleEmployees[Functions.find(clicked, true)]);
				}
			}
		}
	}

	public void displayOrder(Order order, int[] idleEmployees, Employee[] employees){
		this.hasOrder = true;
		this.idleEmployees = idleEmployees;
		this.employees = employees;
		this.order = order;
		string[] orderStrings = Menu.getOrderItemNames(order.order);
		string[] employeeNames = Functions.map((x => x.employeeName), employees);

		currentOrder = Instantiate(orderPrefab, Vector3.zero, Quaternion.identity, transform);
		orderPanel = currentOrder.transform.Find("Order Items").Find("Order Panel").gameObject;
		employeeGrid = currentOrder.transform.Find("Idle Employees").Find("Employees").Find("Employee Grid").gameObject;
		takeButton = currentOrder.transform.Find("Idle Employees").Find("Panel").Find("Take").gameObject.GetComponent<UIButton>();

		orderItems = new GameObject[orderStrings.Length];
		for(int i = 0; i < orderStrings.Length; i++){
			orderItems[i] = Instantiate(orderItemPrefab, Vector3.zero, Quaternion.identity, orderPanel.transform);
			orderItems[i].transform.Find("Text").GetComponent<Text>().text = orderStrings[i];
		}

		employeeGrid.GetComponent<AutoGridLayout>().setRows(idleEmployees.Length / maxColumns + 1);
		employeeItems = new GameObject[idleEmployees.Length];
		for(int i = 0; i < idleEmployees.Length; i++){
			employeeItems[i] = Instantiate(idleEmployeePrefab, Vector3.zero, Quaternion.identity, employeeGrid.transform);
			employeeItems[i].transform.Find("Text").GetComponent<Text>().text = employeeNames[idleEmployees[i]];
		}
	}

	public void addIdleEmployee(int employeeNumber, Employee employee){
		if(!Functions.contains(employeeNumber, idleEmployees)){
			this.idleEmployees = Functions.cat(idleEmployees, new int[]{ employeeNumber });

			GameObject employeeItem = Instantiate(idleEmployeePrefab, Vector3.zero, Quaternion.identity, employeeGrid.transform);
			employeeItem.transform.Find("Text").GetComponent<Text>().text = employee.employeeName;
			this.employeeItems = Functions.cat(employeeItems, new GameObject[]{ employeeItem });

			employeeGrid.GetComponent<AutoGridLayout>().setRows(idleEmployees.Length / maxColumns + 1);
		}
	}

	public void delegateOrder(int employee){
		employees[employee].giveOrder(order);
		clearOrder();
	}

	private void clearOrder(){
		hasOrder = false;
		Destroy(currentOrder);
	}
}
