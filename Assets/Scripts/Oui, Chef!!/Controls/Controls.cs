using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls : MonoBehaviour {

	public static Sprite interact;
	[SerializeField] private Sprite circle;
	[SerializeField] private Sprite cross;
	[SerializeField] private Sprite e;

	private enum controlLayouts : int { Desktop, Dualshock, XBox };

	private int layout = (int) controlLayouts.Desktop;

	public void Start(){
		switch(layout){
			case (int) controlLayouts.Desktop:
				interact = e;				
				break;
			case (int) controlLayouts.Dualshock:
				interact = cross;
				break;
			default:
				interact = e;
				break;
		}
	}
}
