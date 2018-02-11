using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Prompt : MonoBehaviour {

	public Text left;
	public Image image;
	public Text right;
	public Sprite circle;
	private List<PromptMessage> messages;
	private string caller;
	private const char noBreak = '\u00a0';

	public void Start(){
		messages = new List<PromptMessage>();
		//setText("Press the", Controls.interact, "button");
	}

	public void addMessage(string left, Sprite image, string right, string caller){
		//Replace spaces with noBreak
		left = new string(Functions.replace(left.ToCharArray(), ' ', noBreak));
		right = new string(Functions.replace(right.ToCharArray(), ' ', noBreak));
		messages.Add(new PromptMessage(left, image, right, caller));
	}
	// private void setText(string left){
	// 	//Set image and right to inactive
	// 	this.left.gameObject.SetActive(true);
	// 	this.image.transform.parent.gameObject.SetActive(false);
	// 	this.right.gameObject.SetActive(false);

	// 	//Set text
	// 	this.left.text = left;
	// }

	private void setText(string left, Sprite image = null, string right = null){
		bool eitherNull = image == null || right == null;
		//Set left active if either image or right are null; else set all active
		this.left.gameObject.SetActive(true);
		this.image.transform.parent.gameObject.SetActive((!eitherNull) ? true : false);
		this.right.gameObject.SetActive((!eitherNull) ? true : false);

		this.left.text = left;
		if(!eitherNull) {
			this.image.sprite = image;
			this.right.text = right;
		}
	}

	public void setMessage(string caller){
		this.caller = caller;
		
		if(messages.Count == 0){
			this.left.gameObject.SetActive(false);
			this.image.transform.parent.gameObject.SetActive(false);
			this.right.gameObject.SetActive(false);
		}
		else{
			foreach(PromptMessage message in messages){
				if(message.caller == caller) setText(message.left, message.image, message.right);
			}
		}
	}

	public void removeMessage(string caller){
		System.Predicate<PromptMessage> thisCaller = x => x.caller == caller;
		messages.RemoveAll(thisCaller);
		updatePrompt();
		// PromptMessage message = Functions.get(messages.ToArray(), thisCaller, null);
		// if(message != null) messages.Remove(message);
	}

	private void updatePrompt(){
		setMessage(caller);
	}
}

class PromptMessage {
	public string left;
	public Sprite image;
	public string right;
	public string caller;

	public PromptMessage(string left, Sprite image, string right, string caller){
		this.left = left;
		this.image = image;
		this.right = right;
		this.caller = caller;
	}
}