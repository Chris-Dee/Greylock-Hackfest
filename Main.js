#pragma strict

//rift variables 
var riftCenter : GameObject; 
var riftLeft : GameObject;
var riftRight : GameObject; 

//navigation variables 
var speed : float = 0.1f; 

//head-gaze heat map variables 
var gazeFrequencies = new Hashtable(); 
var material : Material = null; 

function Start () {
	riftCenter = GameObject.Find("CenterEyeAnchor"); 
	riftLeft = GameObject.Find("LeftEyeAnchor");
	riftRight = GameObject.Find("RightEyeAnchor"); 
}

function Update () {

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

//function to increment the number of times player "gazes" at an object 
function CheckGaze() {
	var hit : RaycastHit; 
	var ray = new Ray (riftCenter.transform.position, riftCenter.transform.forward);  
	var sourceName : String; 
	if (Physics.Raycast (ray, hit, 100)) {
		sourceName = hit.collider.gameObject.name; 
		//TO DO: check if gameObject has special tag 
		if (!gazeFrequencies.Contains(sourceName)) {
			gazeFrequencies.Add(sourceName, 0); 
		}
		var frequency : int = gazeFrequencies[sourceName]; 
		gazeFrequencies.Add(sourceName, frequency + 1); 
	}
}



