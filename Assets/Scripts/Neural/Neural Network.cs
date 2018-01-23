using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public abstract class NeuralNetwork : IPrintable {

	public abstract void setActivationFunction(string activationFunction);
	public abstract void feedForward(float[] inputs);
	public abstract void train(float[] inputs, float[] desiredOutputs);
	public abstract void train(float[] desiredOutputs);
	public abstract void randomizeWeights(float range);
	public abstract void randomizeBiases(float range);
	public abstract float[] getOutputs();
	public virtual string print(){
		return "Neural Network";
	}
}

public class SimpleNeuralNetwork : NeuralNetwork {
	private List<NeuralNetworkLayer> layers;
	private List<Memory> memories;
	private int retention;

	public SimpleNeuralNetwork(){
		this.layers = new List<NeuralNetworkLayer>();
		this.memories = new List<Memory>();
		this.retention = 0;
	}

	public SimpleNeuralNetwork(int inputs, int outputs, int retention = 0) : this(inputs, new int[]{}, outputs, retention){
	}

	public SimpleNeuralNetwork(int inputs, int[] hiddenLayerSizes, int outputs, int retention = 0){
		this.layers = new List<NeuralNetworkLayer>();
		this.memories = new List<Memory>();
		this.retention = retention;

		if(hiddenLayerSizes.Length > 0){
			layers.Add(new NeuralNetworkLayer(inputs, hiddenLayerSizes[0], activationFunction: "hyperTangent"));
		}

		//Build hidden layers
		for(int i = 1; i < hiddenLayerSizes.Length; i++){
			layers.Add(new NeuralNetworkLayer(hiddenLayerSizes[i - 1], hiddenLayerSizes[i], activationFunction: "hyperTangent"));
		}

		//Build output layer
		if(hiddenLayerSizes.Length > 0){
			layers.Add(new NeuralNetworkLayer(hiddenLayerSizes[hiddenLayerSizes.Length - 1], outputs, activationFunction: "sigmoid"));
		}
		else{
			layers.Add(new NeuralNetworkLayer(inputs, outputs, activationFunction: "sigmoid"));
		}
	}

	public void addLayer(Matrix<float> weightMatrix){
		layers.Add(new NeuralNetworkLayer(weightMatrix));
	}

	public void addLayer(int numberOfInputs, int numberOfNeurons){
		layers.Add(new NeuralNetworkLayer(numberOfInputs, numberOfNeurons));
	}

	public override void setActivationFunction(string activationFunction){
		layers[layers.Count - 1].activationFunction = activationFunction;
	}
	public void setLayerActivationFunction(int layer, string activationFunction){
		layers[layer].activationFunction = activationFunction;
	}

	public override void feedForward(float[] inputs){
		if(layers.Count == 0){
			throw new System.ArgumentException("NeuralNetwork::feedForward ~ no layers in network");
		}

		layers[0].updateOutputs(inputs);

		for(int i = 1; i < layers.Count; i++){
			layers[i].updateOutputs(layers[i - 1].getOutputs());
		}
	}
/*========================================Training System========================================*/
	public override void train(float[] inputs, float[] desiredOutputs){
		feedForward(inputs);
		updateErrors(desiredOutputs);
		updateWeights();
		updateBiases();
	}

	public override void train(float[] desiredOutputs){
		updateErrors(desiredOutputs);
		updateWeights();
		updateBiases();
	}

	public void trainWithErrors(float[] errors){
		setErrors(errors);
		updateWeights();
		updateBiases();
	}

	private void updateErrors(float[] desiredOutputs){
		layers[layers.Count - 1].updateOutputLayerErrors(desiredOutputs);

		for(int i = layers.Count - 2; i >= 0; i--){
			layers[i].updateHiddenLayerErrors(layers[i + 1].weightMatrix, layers[i + 1].errors);
		}
	}

	private void setErrors(float[] errors){
		layers[layers.Count - 1].setOutputLayerErrors(errors);

		for(int i = layers.Count - 2; i >= 0; i--){
			layers[i].updateHiddenLayerErrors(layers[i + 1].weightMatrix, layers[i + 1].errors);
		}
	}

	private void updateWeights(){
		foreach(NeuralNetworkLayer layer in layers){
			layer.updateWeights();
		}
	}

	private void updateBiases(){
		foreach(NeuralNetworkLayer layer in layers){
			layer.updateBiases();
		}
	}
/*===============================================================================================*/

/*========================================Memory System==========================================*/
/* The memory system retains training inputs and outputs for records which the network predicted 
  incorrectly. By default, the network's retention, the number of memories it is able to store is
  set to 0 and the system is thereby disabled. Should retention be increased the network can be 
  trained on its memories by calling ruminate.
  
   The system tends to help networks to generalize by forcing it to specialize in difficult
  records, achieving an effect similar to boosting.	
  
   Rumniate should be called at the beginning of an epoch.
   
   Memories can be added by calling addMemory.          										 */
	public void addMemory(float[] inputs, float[] solution){
		if(this.memories.Count < retention){
			this.memories.Add(new Memory(inputs, solution));
		}
		else{
			if(retention > 0){
				this.memories.Sort(new MemoryComparer());
				this.memories[retention - 1] = new Memory(inputs, solution);
			}
		}
	}

	public int[] getSolutionCounts(){
		int[] counts = new int[memories.Count];
		memories.Sort(new MemoryComparer());

		for(int i = 0; i < counts.Length; i++){
			counts[i] = memories[i].solutionCount;
		}

		return counts;
	}

	public void ruminate(){
		for(int i = 0; i < memories.Count; i++){
			train(memories[i].inputs, memories[i].solution);
			if(favoredNeuron() == Functions.maxIndex(memories[i].solution)){
				memories[i].solutionCount += 1;
			}
			else{
				memories[i].solutionCount = 0;
			}
		}
	}
/*===============================================================================================*/
	public int favoredNeuron(){
		float[] outputs = layers[layers.Count - 1].getOutputs();
		int favored = -1;
		float confidence = float.NegativeInfinity;

		if(outputs.Length == 1) return 0;

		for(int i = 0; i < outputs.Length; i++){
			if(outputs[i] > confidence){
				favored = i;
				confidence = outputs[i];
			}
		}

		return favored;
	}

	public override float[] getOutputs(){
		return layers[layers.Count - 1].getOutputs();
	}

	public float getNeuronValue(int layer, int neuron){
		float[] inputArray = layers[0].getInputs();
		if(inputArray == null){
			return 0.0f;
		}
		else{
			if(layer == 0) return inputArray[neuron];
			return layers[layer - 1].getNeuronValue(neuron);
		}
	}

	public float[] getNeuronWeights(int layer, int neuron){
		return layers[layer - 1].getNeuronWeights(neuron);
	}

	public float getNeuronBias(int layer, int neuron){
		return layers[layer - 1].getNeuronBias(neuron);
	}

	public int getNumberofLayers(){
		return layers.Count + 1;
	}

	public int getNumberofInputs(){
		return layers[0].getNumberOfInputs();
	}

	public int getLayerSize(int layer){
		if(layer == 0) return layers[0].getNumberOfInputs();
		return layers[layer - 1].getNumberOfNeurons();
	}

	public List<string> getActivationFunctions(){
		List<string> activationFunctions = new List<string>(){"none"};

		foreach(NeuralNetworkLayer layer in layers){
			activationFunctions.Add(layer.getActivationFunction());
		}

		return activationFunctions;
	}

	public override void randomizeWeights(float range){
		foreach(NeuralNetworkLayer layer in layers){
			layer.randomizeWeights(range);
		}
	}

	public override void randomizeBiases(float range){
		foreach(NeuralNetworkLayer layer in layers){
			layer.randomizeBiases(range);
		}
	}

	public void setLearningRate(float learningRate){
		foreach(NeuralNetworkLayer layer in layers){
			layer.setLearningRate(learningRate);
		}
	}
	public override string print(){
		string temp  = "Matrix Neural Network\n==============\n";

		foreach(NeuralNetworkLayer layer in layers){
			temp += layer.print();
		}

		return temp;
	}
}

class NeuralNetworkLayer : IPrintable {

	public string activationFunction;
	public Matrix<float> weightMatrix;

	public Vector<float> errors;

	private Vector<float> biases;

	private Vector<float> inputs;

	private Vector<float> activationInputs;

	private Vector<float> outputs;


	private int numberOfInputs;

	private int numberOfNeurons;

	private float learningRate;

	
	private const float e = (float) MathNet.Numerics.Constants.E;

	public NeuralNetworkLayer(int numberOfInputs, int numberOfNeurons, float learningRate = 1.0f, string activationFunction = "sigmoid"){
		this.numberOfInputs = numberOfInputs;
		this.numberOfNeurons = numberOfNeurons;
		this.learningRate = learningRate;
		this.activationFunction = activationFunction;
		this.weightMatrix = Matrix<float>.Build.Dense(numberOfNeurons, numberOfInputs, 0.0f);
		this.biases = Vector<float>.Build.DenseOfArray(Functions.initArray(numberOfNeurons, 0.0f));
		this.inputs = Vector<float>.Build.DenseOfArray(Functions.initArray(numberOfInputs, 0.0f));
		this.outputs = Vector<float>.Build.DenseOfArray(Functions.initArray(numberOfNeurons, 0.0f));
	}

	public NeuralNetworkLayer(Matrix<float> weightMatrix, Vector<float> biases, float learningRate = 1.0f, string activationFunction = "sigmoid"){
		this.numberOfInputs = weightMatrix.ColumnCount;
		this.numberOfNeurons = weightMatrix.RowCount;
		this.learningRate = learningRate;
		this.weightMatrix = weightMatrix;
		this.activationFunction = activationFunction;
		this.biases = biases;
		this.inputs = Vector<float>.Build.Dense(numberOfInputs, 0.0f);
		this.outputs = Vector<float>.Build.DenseOfArray(Functions.initArray(numberOfNeurons, 0.0f));	
	}

	public NeuralNetworkLayer(Matrix<float> weightMatrix, float learningRate = 1.0f, string activationFunction = "sigmoid"){
		this.numberOfInputs = weightMatrix.ColumnCount;
		this.numberOfNeurons = weightMatrix.RowCount;
		this.learningRate = learningRate;
		this.weightMatrix = weightMatrix;
		this.activationFunction = activationFunction;
		this.biases = Vector<float>.Build.DenseOfArray(Functions.initArray(numberOfNeurons, 0.0f));
		this.inputs = Vector<float>.Build.DenseOfArray(Functions.initArray(numberOfInputs, 0.0f));
		this.outputs = Vector<float>.Build.DenseOfArray(Functions.initArray(numberOfNeurons, 0.0f));
	}


	public void randomizeWeights(float range){
		for(int i = 0; i < weightMatrix.RowCount; i++){
			for(int j = 0; j < weightMatrix.ColumnCount; j++){
				weightMatrix[i,j] = Random.Range(-range, range);
			}
		}
	}

	public void randomizeBiases(float range){
		float[] randoms = new float[numberOfNeurons];

		for(int i = 0; i < randoms.Length; i++){
			randoms[i] = Random.Range(-range, range);
		}

		this.biases = Vector<float>.Build.DenseOfArray(randoms);
	}

	public void updateOutputs(float[] inputs){
		if(inputs.Length == weightMatrix.ColumnCount){
			this.inputs = Vector<float>.Build.DenseOfArray(inputs);
			activationInputs = weightMatrix.Multiply(this.inputs);
			activationInputs = activationInputs.Add(biases);
			if(activationFunction == "hyperTangent"){
				outputs = activationInputs.Map(x => (float)MathNet.Numerics.Trig.Tanh(x));
			}
			else if(activationFunction == "sigmoid"){
				outputs = activationInputs.Map(x => sigmoid(x));
			}
			else if(activationFunction == "softmax"){
				float[] scaledActivationInputs = activationInputs.AsArray();
				float largestOutput = Functions.max(scaledActivationInputs);
				scaledActivationInputs = Functions.map((x => x - largestOutput), scaledActivationInputs);
				float sum = Functions.sum(Functions.map((x => (x > -5) ? Mathf.Exp(x) : Mathf.Exp(-5)), scaledActivationInputs));
				Debug.Log(sum);
				outputs = activationInputs.Map(x => softmax(sum, x));
			}
			else{
				outputs = activationInputs;
			}
		}
		else{
			throw new System.ArgumentException("NeuralNetworkLayer::updateOutputs ~ Mismatched input length and weight matrix size\nInput length: " + inputs.Length.ToString() + " Weight matrix columns: " + weightMatrix.ColumnCount.ToString());
		}
	}

	public void updateOutputLayerErrors(float[] desiredOutputs){
		if(desiredOutputs.Length != numberOfNeurons){
			throw new System.ArgumentException("NeuralNetworkLayer::updateOutputLayerErrors ~ Mismatched desired output length and number of neurons\nDesired output length: " + desiredOutputs.Length.ToString() + ", Number of neurons: " + numberOfNeurons.ToString());
		}
		if(outputs == default(Vector<float>) || activationInputs == default(Vector<float>)){
			throw new System.Exception("NeuralNetworkLayer::updateOutputLayerErrors ~ Either layer activation inputs or outputs has not been initialized");
		}

		Vector<float> targetVector = Vector<float>.Build.DenseOfArray(desiredOutputs);
		Vector<float> dOutput;

		if(activationFunction == "hyperTangent"){
			dOutput = activationInputs.Map(x => dTanh(x));
		}
		else if(activationFunction == "sigmoid"){
			dOutput = activationInputs.Map(x => dSigmoid(x));
		}
		else if(activationFunction == "softmax"){
			dOutput = Vector<float>.Build.DenseOfArray(Functions.zipWith(((x,y) => dSoftmax(x, y)), activationInputs.AsArray(), desiredOutputs));
		}
		else{
			dOutput = activationInputs;
		}


		Vector<float> dOutputErrors = outputs.Subtract(targetVector);
		if(activationFunction == "none"){
			this.errors = dOutputErrors;
		}
		else{
			this.errors = dOutputErrors.PointwiseMultiply(dOutput);
		}
	}

	public void setOutputLayerErrors(float[] errors){
		this.errors = Vector<float>.Build.DenseOfArray(errors);
	}

	public void updateHiddenLayerErrors(Matrix<float> upstreamWeights, Vector<float> upstreamErrors){
		if(numberOfNeurons != upstreamWeights.ColumnCount){
			throw new System.Exception("MatrixNerualNetworkLayer::updateHiddenLayerErrors ~ Mismatch number of neurons and upstream weight rows\nNumber of neurons: " + numberOfNeurons.ToString() + ", Number of upstream weight rows: " + upstreamWeights.RowCount.ToString());
		}

		if(activationFunction == "hyperTangent"){	
			this.errors = upstreamWeights.Transpose().Multiply(upstreamErrors).Map(x => x / upstreamErrors.Count).PointwiseMultiply(activationInputs.Map(x => dTanh(x)));
		}
		else if(activationFunction == "sigmoid"){
			this.errors = upstreamWeights.Transpose().Multiply(upstreamErrors).Map(x => x / upstreamErrors.Count).PointwiseMultiply(activationInputs.Map(x => dSigmoid(x)));
		}
		else{
			this.errors = upstreamWeights.Transpose().Multiply(upstreamErrors).Map(x => x / upstreamErrors.Count);
		}
	}

	public void updateWeights(){
		Matrix<float> steps = Matrix<float>.Build.DenseOfColumnVectors(Functions.initArray(numberOfInputs, errors));
		steps = steps.Multiply(Matrix<float>.Build.DiagonalOfDiagonalVector(inputs));
		steps = steps.Multiply(learningRate);
		weightMatrix = weightMatrix.Subtract(steps);
	}

	public void updateBiases(){
		biases = biases.Subtract(errors.Multiply(learningRate));
	}

	public void setLearningRate(float learningRate){
		this.learningRate = learningRate;
	}

	public float[] getInputs(){
		return this.inputs.AsArray();
	}
	public float[] getOutputs(){
		if(outputs == default(Vector<float>)) return Functions.initArray(numberOfNeurons, 0.0f);
		return this.outputs.AsArray();
	}

	public float getNeuronValue(int neuron){
		return outputs[neuron];
	}

	public float[] getNeuronWeights(int neuron){
		return weightMatrix.Row(neuron).ToArray();
	}

	public float getNeuronBias(int neuron){
		return biases[neuron];
	}

	public int getNumberOfInputs(){
		return numberOfInputs;
	}

	public int getNumberOfNeurons(){
		return numberOfNeurons;
	}

	public string getActivationFunction(){
		return this.activationFunction;
	}

/*======================================Activation Functions=====================================*/
/* These are the available activation functions and their derivatives. The hypertangent function is
  defined in Unity's Mathf package so only its derivative is featured here.    					 */
	private float sigmoid(float x){
		if(x > 10.0f) return 0.9999f;
		if(x < -10.0f) return -0.9999f;
		
		return (2.0f / (1 + Mathf.Pow(e, -x))) - 1.0f; 
	}

	private float softmax(float sum, float x){
		Debug.Log("Softmax: sum: " + sum.ToString() + ", x: " + x.ToString());
		return (Mathf.Exp((x > -5) ? x : -5)) / sum;
	}

	private float dTanh(float x){
		return Mathf.Pow((float)MathNet.Numerics.Trig.Sech(x), 2);
	}

	private float dSigmoid(float x){
		if(Mathf.Abs(x) > 10.0f) return 0.0001f;
		return (2.0f * Mathf.Pow(e, x)) / Mathf.Pow((Mathf.Pow(e, x) + 1.0f), 2.0f);
	}

	private float dSoftmax(float x, float y){
		return x - y;
	}

/*===============================================================================================*/
	public string print(){
		string temp = "Neural Network Layer\n==============\nNumber of neurons: " + numberOfNeurons.ToString() + "\n\n";
		temp += "Weight Matrix\n=========\n" + Functions.print(weightMatrix) + "\n";
		temp += "Biases\n======\n" + Functions.print(biases) + "\n";
		temp += "Errors\n======\n" + Functions.print(errors);
		return temp;
	}
}


public class Memory : IPrintable{
	public float[] inputs;
	
	public float[] solution;

	public int solutionCount;

	public Memory(float[] inputs, float[] solution, int solutionCount = 0){
		this.inputs = inputs;
		this.solution = solution;
		this.solutionCount = solutionCount;
	}

	public string print(){
		return solutionCount.ToString();
	}
}

public class MemoryComparer : IComparer<Memory>{

	int IComparer<Memory>.Compare(Memory x, Memory y){
		if(x.solutionCount == y.solutionCount){
			return 0;
		}
		else{
			return (x.solutionCount < y.solutionCount) ? -1 : 1;
		}
	}
}