using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Splash : MonoBehaviour {

	[SerializeField] private float splashTime;

	private float timeElapsed = 0.0f;
	void Start () {
		timeElapsed = 0.0f;
	}
	
	void Update () {
		timeElapsed += Time.deltaTime;
		if(timeElapsed > splashTime){
			SceneManager.LoadScene("Scenes/Learning Test", LoadSceneMode.Single);
		}
	}
}
