using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LearningTest : MonoBehaviour {

	[SerializeField] GameObject orderPanel;
	[SerializeField] GameObject recipePanel;
	[SerializeField] GameObject buttonGrid;
	[SerializeField] GameObject textGrid;
	[SerializeField] GameObject buttonColumnPrefab;
	[SerializeField] GameObject buttonPrefab;
	[SerializeField] GameObject textPrefab;
	[SerializeField] GameObject[] buttonColumns;
	[SerializeField] GameObject[] buttons;
	[SerializeField] GameObject[] textColumns;
	[SerializeField] GameObject[] ingredientText;
	[SerializeField] GameObject neuralNetworkView;
	[SerializeField] GameObject trainButton;
	[SerializeField] GameObject runButton;
	[SerializeField] GameObject scoreText;
	[SerializeField] bool[] clicked;
	private Menu menu;
	private Recipes recipes;
	private SimpleNeuralNetwork employee;
	private const int uiColumns = 4;
	private bool train;
	private bool run;
	private int iteration = 0;
	private float[] scores = Functions.initArray(100, 0.0f);
	[SerializeField] private Color white;
	[SerializeField] private Color blue;
	[SerializeField] private Color red;
	[SerializeField] private Color green;

	void Start () {
		train = true;
		run = false;
		Menu.setMenu(new int[]{ 
			(int) Menu.items.Gnocchi,
			(int) Menu.items.Pizza,
			(int) Menu.items.Frittata
		});
		recipes = new Recipes();
		//employee = new SimpleNeuralNetwork((int) Menu.items.Count, new int[]{Mathf.FloorToInt(Mathf.Sqrt(Mathf.Max((float) Menu.items.Count, (float) Recipes.ingredients.Count)))}, (int) Recipes.ingredients.Count);
		//employee = new SimpleNeuralNetwork((int) Menu.items.Count, new int[]{Mathf.FloorToInt(Mathf.Max((float) Menu.items.Count, (float) Recipes.ingredients.Count))}, (int) Recipes.ingredients.Count);
		//employee = new SimpleNeuralNetwork((int) Menu.items.Count, Functions.initArray(3,Mathf.FloorToInt(Mathf.Max((float) Menu.items.Count, (float) Recipes.ingredients.Count))), (int) Recipes.ingredients.Count);
		employee = new SimpleNeuralNetwork((int) Menu.items.Count, (int) Recipes.ingredients.Count);
		employee.setActivationFunction("softmax");
		employee.randomizeWeights(0.1f);
		employee.randomizeBiases(0.1f);
		employee.setLearningRate(0.5f);

		buttonColumns = new GameObject[uiColumns];
		buttons = new GameObject[(int) Menu.items.Count];
		clicked = Functions.initArray((int) Menu.items.Count, false);
		textColumns = new GameObject[uiColumns];
		ingredientText = new GameObject[(int) Recipes.ingredients.Count];
		neuralNetworkView = Instantiate(neuralNetworkView, Vector3.zero, Quaternion.identity);
		neuralNetworkView.GetComponent<NeuralNetworkView>().setNeuralNetwork(employee, "center");

		string[] menuItems = System.Enum.GetNames(typeof(Menu.items));
		string[] ingredients = System.Enum.GetNames(typeof(Recipes.ingredients));

		for(int i = 0; i < menuItems.Length - 1; i++){
			if(i < uiColumns){
				buttonColumns[i] = Instantiate(buttonColumnPrefab, Vector3.zero, Quaternion.identity, buttonGrid.transform);
				buttons[i] = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, buttonColumns[i].transform);
			}
			else{
				buttons[i] = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, buttonColumns[i % uiColumns].transform);
			}

			buttons[i].GetComponent<Image>().color = white;
			buttons[i].transform.Find("Text").GetComponent<Text>().text = menuItems[i];
			int buttonIndex = i;
			buttons[i].GetComponent<Button>().onClick.AddListener(delegate {click(buttonIndex);});
		}

		for(int i = 0; i < ingredients.Length - 1; i++){
			if(i < uiColumns){
				textColumns[i] = Instantiate(buttonColumnPrefab, Vector3.zero, Quaternion.identity, textGrid.transform);
				ingredientText[i] = Instantiate(textPrefab, Vector3.zero, Quaternion.identity, textColumns[i].transform);
			}
			else{
				ingredientText[i] = Instantiate(textPrefab, Vector3.zero, Quaternion.identity, textColumns[i % uiColumns].transform);
			}

			ingredientText[i].GetComponent<Image>().color = white;
			ingredientText[i].transform.Find("Text").GetComponent<Text>().text = ingredients[i];
		}

		updateButtonColors();
	}
	
	void Update () {
		if(!Functions.all((x => !x), clicked)){
			List<int> order = new List<int>();
			iteration++;
			iteration = iteration % 100;
			
			for(int i = 0; i < clicked.Length; i++){
				if(clicked[i]) order.Add(i);
			}

			int[] employeeChoices = testEmployee(order.ToArray());
			updateOrderButtonColors(order.ToArray());

			int[] recipe = Functions.map((x => (int) x), Recipes.getRecipe(order.ToArray()));
			int[] choices = Functions.initArray(recipe.Length, -1);

			for(int i = 0; i < employeeChoices.Length; i++){
				choices[employeeChoices[i]] = 1;
			}
			updateTextColors(choices, recipe);

			clicked = Functions.map((x => false), clicked);

			int correctIngredients = 0;

			for(int i = 0; i < recipe.Length; i++){
				if(recipe[i] == choices[i] && recipe[i] == 1) correctIngredients++;
			}

			scores[iteration] = correctIngredients / ((float) Functions.count(1, recipe));
			scoreText.transform.Find("Text").GetComponent<Text>().text = Functions.mean(scores).ToString("0.00");
		}
		else{
			if(run){
				List<int> order = new List<int>();
				iteration++;
				iteration = iteration % 100;
				order.Add((int) Random.Range(0.0f, (int) Menu.items.Count));
				int[] employeeChoices = testEmployee(order.ToArray());
				updateOrderButtonColors(order.ToArray());

				int[] recipe = Functions.map((x => (int) x), Recipes.getRecipe(order.ToArray()));
				int[] choices = Functions.initArray(recipe.Length, -1);

				for(int i = 0; i < employeeChoices.Length; i++){
					choices[employeeChoices[i]] = 1;
				}
				updateTextColors(choices, recipe);

				int correctIngredients = 0;
				for(int i = 0; i < recipe.Length; i++){
					if(recipe[i] == choices[i] && recipe[i] == 1) correctIngredients++;
				}

				scores[iteration] = correctIngredients / ((float) Functions.count(1, recipe));
				scoreText.transform.Find("Text").GetComponent<Text>().text = Functions.mean(scores).ToString("0.00");
			}
		}
	}

	private void updateOrderButtonColors(int[] order){
		foreach(GameObject button in buttons){
			button.GetComponent<Image>().color = white;
		}

		for(int i = 0; i < order.Length; i++){
			buttons[order[i]].GetComponent<Image>().color = blue;
		}
	}

	private void updateButtonColors(){
		if(train) trainButton.GetComponent<Image>().color = green;
		else trainButton.GetComponent<Image>().color = red;

		if(run) runButton.GetComponent<Image>().color = green;
		else runButton.GetComponent<Image>().color = red;
	}

	private void updateTextColors(int[] employeeChoices, int[] recipe){
		if(employeeChoices.Length != recipe.Length) throw new System.ArgumentException("LearningText::updateTextColors ~ non matching argument array lengths\nEmployee Choices: " + Functions.print(employeeChoices) + "\nRecipe: " + Functions.print(recipe));

		for(int i = 0; i < employeeChoices.Length; i++){
			if(employeeChoices[i] == recipe[i] && employeeChoices[i] == 1) ingredientText[i].GetComponent<Image>().color = green;
			else if(employeeChoices[i] == 1) ingredientText[i].GetComponent<Image>().color = red;
			else if(recipe[i] == 1) ingredientText[i].GetComponent<Image>().color = blue;
			else ingredientText[i].GetComponent<Image>().color = white;
		}
	}

	private int[] testEmployee(int[] order){
		float[] inputs = Menu.createOrder(order);
		float[] recipe = Recipes.getRecipeVector(order);

		employee.feedForward(inputs);

		float[] outputs = employee.getOutputs();
		int numberOfIngredients = Functions.count(1.0f, recipe);
		int[] employeeChoices = Functions.getNGreatestIndices(outputs, numberOfIngredients);

		if(train){
			employee.train(recipe);
		}

		return employeeChoices;
	}

	public void click(int index){
		clicked[index] = true;
	}

	public void toggleTrain(){
		train = !train;
		updateButtonColors();
	}

	public void toggleRun(){
		run = !run;
		updateButtonColors();
	}
}
