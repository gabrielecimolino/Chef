using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Station : MonoBehaviour {

	public string stationName;
	public Collider2D chefTrigger;
	public Kitchen kitchen;

	public abstract void interact();
	public abstract float employeeInteract();
}
