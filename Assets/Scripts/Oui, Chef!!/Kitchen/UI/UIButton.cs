using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButton : MonoBehaviour {

	[SerializeField] private bool clicked;

	public void click(){
		clicked = true;
	}

	public bool getClicked(){
		if(clicked){
			clicked = false;
			return true;
		} 

		return false;
	}
}
