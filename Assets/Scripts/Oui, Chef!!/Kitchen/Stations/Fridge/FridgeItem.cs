using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FridgeItem : MonoBehaviour {

	public bool target = false;
	public bool selected = false;
	[SerializeField] private Text text;
	[SerializeField] private string itemName;
	[SerializeField] private Image image;
	[SerializeField] private UIButton button;
	[SerializeField] private Color defaultColor;

	void Start () {
		defaultColor = image.color;
	}
	
	void Update () {
		if(button.getClicked()){
			selected = !selected;
			if(selected) image.color = Color.green;
			else image.color = defaultColor;
		}
	}

	public void setText(string text){
		//AAAAAAAYYYY LMAO
		itemName = text;
		this.text.text = text;
	}

	public void reset(){
		this.target = false;
		this.selected = false;
		image.color = defaultColor;
	}
}
