#pragma strict
import System.Collections.Generic;

//rift variables 
var riftCenter : GameObject; 
var riftLeft : GameObject;
var riftRight : GameObject; 

//navigation variables 
var speed : float = 0.1f; 

//head-gaze heat map variables 
var userTag : String = "UserTag"; 
var gazeFrequencies = new Dictionary.<String, int>(); 
var standardMat : Material = null; 

function Start () {
	AddMeshCollider();
	riftCenter = GameObject.Find("CenterEyeAnchor"); 
	riftLeft = GameObject.Find("LeftEyeAnchor");
	riftRight = GameObject.Find("RightEyeAnchor"); 
}

function Update () {
	//UNCOMMENT WHEN RIFT ATTACHED: MovePlayer();
	CheckGaze(); 
	if (NormalizeFrequencies() > -1) {
		UpdateColorMap(); 
	}
}

//function to move player forward based on head gaze 
function MovePlayer() {
	//get rift transform 
	var riftTransform = riftCenter.transform; 
	//if left mouse button clicked and rift is not null
	if (Input.GetMouseButton (0) && riftCenter != null) {
		//move player forward using ray cast by rift 
		transform.position += riftTransform.forward * speed; 
	}
}

//function to add mesh renderers to objects so they can be detected for collisions 
function AddMeshCollider() {
	var gameObjects : GameObject[] = FindObjectsOfType(GameObject) as GameObject[];
	for (var object in gameObjects) {
		object.AddComponent.<MeshCollider>(); 
		Debug.Log("Added mesh renderer to: " + object.name); 
	}
}

//function to increment the number of times player "gazes" at an object 
function CheckGaze() {
	var hit : RaycastHit; 
	//UNCOMMENT WHEN RIFT ATTACHED: var ray = new Ray (riftCenter.transform.position, riftCenter.transform.forward);  
	var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	var source : GameObject; 
	var sourceName : String; 
	if (Physics.Raycast (ray, hit, 100)) {
		source = hit.collider.gameObject; 
		sourceName = source.name; 
		Debug.Log("Hit object: " + sourceName); 
		//UNCOMMENT AFTER TAGGING OBJECTS 
		//if (source.tag.Equals(userTag))
		if (!gazeFrequencies.ContainsKey(sourceName)) {
			gazeFrequencies.Add(sourceName, 0); 
		}
		var frequency = gazeFrequencies[sourceName] + 1; 
		gazeFrequencies.Remove(sourceName); 
		gazeFrequencies.Add(sourceName, frequency); 
		Debug.Log("Gaze frequency: " + gazeFrequencies[sourceName]); 
	}
}

//get gaze frequency of "most-gazed-at" object 
function GetMaxValue() {
	var maxKey : String = ""; 
	var maxVal : int = 0; 
	for (var key in gazeFrequencies.Keys) {
		var val = gazeFrequencies[key]; 
		if (val > maxVal) {
			maxKey = key; 
			maxVal = val; 
		}
	}
	return maxVal; 	
}

//normalize frequencies of gaze to 255 
function NormalizeFrequencies() {
	var maxValue : int = GetMaxValue(); 
	if (maxValue <= 0) return -1; 
	else {
		for (var key in gazeFrequencies.Keys) {
			Debug.Log("Max Value: " + maxValue); 
			Debug.Log("Current Val: " + gazeFrequencies[key]); 
			var normalized = gazeFrequencies[key]/(Mathf.Ceil(maxValue/255)); 
			Debug.Log("Mathf.Ceil(maxValue/255): " + Mathf.Ceil(maxValue/255));
			var original = gazeFrequencies[key];
			original = normalized; 
			Debug.Log("Normalized Val: " + original); 
		}
		return 0; 
	}
}

//update color of objects according to normalized gaze frequencies 
function UpdateColorMap() {
	for (var key in gazeFrequencies.Keys) {
		var object : GameObject = GameObject.Find(key); 
		if (object != null) {
			var renderer : Renderer = object.GetComponent.<Renderer>(); 
			var redVal : byte = gazeFrequencies[key]; 
			var newColor : Color = new Color32(redVal, 0, 255-redVal, 0); 
			renderer.material.SetColor("_Color", newColor); 
		}
	}
}



