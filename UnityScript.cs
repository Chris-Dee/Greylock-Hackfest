using System.Linq; 

public class CreateHeatmap : MonoBehaviour {

	public float logsPerSecond = 1f;
	private float logSplit;// Default to one log per second
	private float timer = 0f;
	public List<Vector3> points = new List<Vector3>();
	public string HeatmapTextAssetPath = "Assets/PlayerPoints.txt";
	public TextAsset heatmapFile; 
	public Camera camera1; 
	public Camera rift1;
	public Camera rift2;
	public Camera camera2; 
	private Dictionary<string, int> gazeFrequencies = new Dictionary<string, int>(); 

	string variationDataInfo = "/resources/variation-info.txt";
	string testDataInfo = "/Resources/testInfo.txt";
	string flagInfo = "/resources/mode.txt";
	string url = "http://127.0.0.1:9000";
	string variationId = "";
	string experimentId = "";
	string post_url = "";
	bool isUser;
	
	public void Start()
	{
		readDataFile ();
		float logSplit = 1f/logsPerSecond; 
		readFromDataFile ();
		variationId = getVariationIdFromFile ();
		experimentId = getExperimentIdFromFile ();
		post_url = url + "/api/experiments/" + experimentId + "/variations/" + variationId; 
		isUser = checkForUser ();
		Debug.Log ("url: " + post_url);
		postRequest ();
	}
	

	public void Update()
	{
		timer += Time.deltaTime;
		
		if(timer > logSplit)
		{
			timer = 0f;
			LogPosition(gameObject.transform.position);
		}
		CheckQuit ();
	}
	
	public void OnDisable()
	{
		postRequest ();
	}

	public void ReadPositionalFile() {
		Vector3[] positionArray = StringUtility.Vector3ArrayWithFile(heatmapFile);
		for (int i = 0; i < positionArray.Length; i++) {
			Debug.Log ("Reading from file: " + positionArray[i].x + " " + positionArray[i].y + " " + positionArray[i].z); 
			points.Add (positionArray[i]); 
		}
	}

	bool checkForUser(){
		string user = File.ReadAllText (Application.dataPath + "/" + flagInfo);
		return (user == "user"); 
	}

	void postRequest() {
		WWW www = new WWW(post_url, createRequest());
		StartCoroutine(WaitForRequest(www));
	}

	IEnumerator WaitForRequest(WWW www)
	{
		yield return www;
		if (www.error == null)
		{
			Debug.Log("WWW Ok!: " + www.data);
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}    
	}

	WWWForm createRequest(){
		WWWForm form = new WWWForm ();
		Debug.Log ("positions: " + serializePositions (points));
		form.AddField("position", serializePositions(points));
//		form.AddField("gaze", serializeGaze(generateVar()));
		Debug.Log (form.data);
		return form; 
	}

	string serializePositions(List<Vector3> pos){
		string parsed = "";
		foreach(Vector3 vec in pos){
			float x = vec.x;
			float y = vec.y;
			float z = vec.z;
			parsed += "("+x.ToString()+", "+y.ToString()+", "+z.ToString()+")\n";
		}
		return parsed;
	}
	string serializeGaze(Dictionary<string,int> gaze){
		string parsed = "";
		foreach (string obj in gaze.Keys) {
			int freq = 0;
			gaze.TryGetValue (obj, out freq);
			parsed += "\n" + obj + " " + freq.ToString ();
		}
		return parsed;
	}
	
	public void LogPosition(Vector3 position)
	{
		points.Add(position);
	}

	Rect screenshotRect = new Rect(5, 40, 100, 30);
	
	public void OnGUI()
	{
		if(GUI.Button(screenshotRect, "Screenshot"))
		{
			camera1.enabled = false;
			camera2.enabled = true; 

			Debug.Log (camera2.isActiveAndEnabled); 

			// Positions / Camera / How large to draw heat points
			Texture2D heatmapImage = Heatmap.CreateHeatmap(points.ToArray (),
			                                               camera2, 40);
			
			// Draw the Heatmap in front of the camera
			Heatmap.CreateRenderPlane(heatmapImage);
			
			// And take the screenshot!
			Heatmap.Screenshot("Assets/MyScreenshot.png");
		}
		StringUtility.Vector3ArrayToTextAsset(points.ToArray(), HeatmapTextAssetPath);
	}

	void readDataFile(){
		string filepath = Application.dataPath + "/" + testDataInfo;
		string str = File.ReadAllText (filepath);
		string[] data = str.Split (new string[] {")", "("}, System.StringSplitOptions.RemoveEmptyEntries);
		foreach (string line in data) {
			string[] datum = str.Split (new string[] {","}, System.StringSplitOptions.RemoveEmptyEntries);
			foreach(string ind in datum){
				Debug.Log(ind);
			}
		}
	}

	void readFromDataFile(){
		string filepath = Application.dataPath + "/" + testDataInfo;
		string[] lines = File.ReadAllLines (filepath);
		string lastIdentifier = "";
		foreach(string line in lines){
			string[] data = line.Split (new string[] {" "}, System.StringSplitOptions.RemoveEmptyEntries);
			if(line.Contains(":")){
				lastIdentifier = data[0];
			}
			if(data.Length == 3 && lastIdentifier == "position"){
				Vector3 currVector = new Vector3(float.Parse(data[0]), float.Parse (data[1]), float.Parse (data[2]));
				points.Add (currVector);
				}
			if (data.Length == 2 && lastIdentifier == "gaze") {
				Debug.Log("gaze parsed: " + data);
				gazeFrequencies.Add(data[0], int.Parse(data[1]));
			}
		}
	}

	void ParseString(){
		
		//Reading from Resources folder 
		//CANT DO THIS
		TextAsset tempAsset = (TextAsset) Resources.Load("info"); 
		string str = tempAsset.text; 
		Debug.Log (str); 
		string[] lines = str.Split (new string[] {"\r\n", "\n"}, System.StringSplitOptions.None); 
		foreach (string s in lines) {
			Debug.Log ("split string: " + s); 
			string[] data = s.Split (new string[] {" "}, System.StringSplitOptions.None); 
			GameObject obj = GameObject.Find (data[0]);
			byte redVal = (byte) int.Parse(data[1]); 
			Debug.Log ("Original r: " + redVal);
			obj.GetComponent<Renderer>().material.color = new Color32(redVal, 0, (byte) (255-redVal), 0); 
		} 
		string word = File.ReadAllText(Application.dataPath + "/info.txt");
		Debug.Log (word); 
	}



	int GetMaxVal() {
		string maxKey = ""; 
		int maxVal = 0; 
		foreach (string key in gazeFrequencies.Keys) {
			int val = 0;
			gazeFrequencies.TryGetValue (key, out val); 
			if (val > maxVal) {
				maxKey = key; 
				maxVal = val; 
			}
		}
		return maxVal; 
	}

	string getVariationIdFromFile(){
		string str = File.ReadAllText (Application.dataPath + variationDataInfo);
		string match = "\"_id\"";
		int i = str.IndexOf(match) + match.Length + 2;
		int endIndex = str.Substring (i).IndexOf ('\"');
		str.Substring (i, endIndex);
		Debug.Log ("index: " + str.Substring (i,endIndex));
		return str.Substring (i, endIndex);
	}

	string getExperimentIdFromFile(){
		string str = File.ReadAllText (Application.dataPath + variationDataInfo);
		string match = "\"experimentId\"";
		int i = str.IndexOf (match) + match.Length + 2;
		int endIndex = str.Substring(i).IndexOf('\"');
		Debug.Log ("index: " + str.Substring (i, endIndex));
		return str.Substring (i, endIndex);
	}
	
	void NormalizeFrequencies() {
		int maxVal = GetMaxVal (); 
		int val = 0;
		//if maxVal is 0, something went wrong. Set all values in dictionary to 0. Server can deal with error. 
		if (maxVal == 0) {
			foreach (string key in gazeFrequencies.Keys) {
				gazeFrequencies[key] = 0;
				return;
			}
		}
		foreach (string key in gazeFrequencies.Keys) {
			int oldVal = gazeFrequencies[key];
			gazeFrequencies[key] = oldVal/(maxVal/255); 
		}
	}
	void checkGaze(){
		GameObject rift = GameObject.Find ("CenterEyeAnchor"); 
		RaycastHit hit; 
		Ray ray = new Ray (rift.transform.position, rift.transform.forward);  
		if (Physics.Raycast (ray, out hit)) {
			GameObject source = hit.collider.gameObject; 
			int value = 0;
			if (gazeFrequencies.ContainsKey (source.name)) {
				if (gazeFrequencies.TryGetValue (source.name, out value)) {
					gazeFrequencies [source.name] = value + 1; 
				}
			} else {
				gazeFrequencies.Add (source.name, 1); 
			}
			}
		}
	public void CheckQuit() {
		if(Input.GetKey (KeyCode.Escape))
		{
			rift1.enabled = false;
			rift2.enabled = false; 
			camera2.enabled = true; 
			
			Debug.Log (camera2.isActiveAndEnabled); 
			
			// Positions / Camera / How large to draw heat points
			Texture2D heatmapImage = Heatmap.CreateHeatmap(points.ToArray (),
			                                               camera2, 20);
			// Draw the Heatmap in front of the camera
			Heatmap.CreateRenderPlane(heatmapImage);
			
			// And take the screenshot!
			Heatmap.Screenshot("Assets/MyScreenshot.png");
		}
		StringUtility.Vector3ArrayToTextAsset(points.ToArray(), HeatmapTextAssetPath);
		Application.Quit (); 
	}
//	public void sendPicture(){
//		WWW file = new WWW (Application.dataPath + "/Assets/MyScreenshot.png");
//		yield return localFile;
//		WWWForm form = new WWWForm ();
//		form.AddBinaryData();
//
//	}

	IEnumerator UploadFileCo(string uploadURL)
	{
		WWW localFile = new WWW("Assets/MyScreenshot.png");
		yield return localFile;
		WWWForm postForm = new WWWForm();
		postForm.AddBinaryData("heatmap", localFile.bytes, "MyScreenshot.png", "image/png");
		WWW upload = new WWW(uploadURL, postForm);
		yield return upload;
		if (upload.error == null)
		{
			Debug.Log(upload.text);
		}
		else
		{
			Debug.Log("Error during upload: " + upload.error);
		}
	}
}
