using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class BackCamera : MonoBehaviour {
    
	//crowdsourcing
	public RawImage photoImage;
	public GameObject texterror1;
	public RawImage urlImage;
	private string m_LocalFileName = "C:/boot.ini";
	private string m_URL = "http://109.234.35.213/upload.php";
	public string[] items;
	//-------------------------------


	private RawImage image;
    private WebCamTexture cam;
    private AspectRatioFitter arf;

	// Use this for initialization
	void Start () {
        arf = GetComponent<AspectRatioFitter> ();

        image = GetComponent<RawImage> ();
        cam = new WebCamTexture (Screen.width, Screen.height);
        image.texture = cam;
        cam.Play ();
	}
	
	// Update is called once per frame
	void Update () {
        if (cam.width < 100) {
            return;     
        }

        float cwNeeded = -cam.videoRotationAngle;
        if (cam.videoVerticallyMirrored)
            cwNeeded += 180f;

        image.rectTransform.localEulerAngles = new Vector3 (0f, 0f, cwNeeded);

        float videoRatio = (float)cam.width / (float)cam.height;
        arf.aspectRatio = videoRatio;
	}
		
	IEnumerator UploadFileCo()
	{
		Texture2D photo = new Texture2D(cam.width, cam.height);
		photo.SetPixels(cam.GetPixels());
		photo.Apply();
		byte[] bytes = photo.EncodeToPNG();
		File.WriteAllBytes(Application.persistentDataPath +"/" + "photo.png", bytes);
		m_LocalFileName = Application.persistentDataPath + "/" + "photo.png";

		WWW localFile = new WWW("file:///" + m_LocalFileName);
		yield return localFile;
		if (localFile.error == null)
			Debug.Log("Loaded file successfully");
		else
		{
			Debug.Log("Open file error: " + localFile.error);
			yield break; // stop the coroutine here
		}
		WWWForm postForm = new WWWForm();
		// version 1
		//postForm.AddBinaryData("theFile",localFile.bytes);
		// version 2
		postForm.AddBinaryData("theFile", localFile.bytes, m_LocalFileName, "text/plain");
		postForm.AddField("lon", "59.99345");
		postForm.AddField("lat", "30.14687");
		postForm.AddField("comment", "Info");
		WWW upload = new WWW(m_URL, postForm);
		yield return upload;
		if (upload.error == null)
		{
			Debug.Log("upload done :" + upload.text);
		}
		else
		{
			Debug.Log("Error during upload: " + upload.error);
		}

	}
	public void UploadFile()
	{
		StartCoroutine(UploadFileCo());
	}


	public IEnumerator GetFileCo()
	{
		WWW itemsData = new WWW("http://109.234.35.213/getfile.php");
		yield return itemsData;
		string itemsDataString = itemsData.text;
		print(itemsDataString);
		items = itemsDataString.Split('|');

		Texture2D tex;
		tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
		WWW www = new WWW("http://109.234.35.213/images/photo.png");
		yield return www;
		www.LoadImageIntoTexture(tex);
		urlImage.GetComponent<RawImage>().texture = tex;
		texterror1.GetComponent<Text>().text = items[0].ToString()+","+items[1].ToString();
	}
	public void GetFile()
	{
		if (urlImage.enabled == true) {
			texterror1.GetComponent<Text>().enabled = false;
			urlImage.enabled = false;
		} else {
			texterror1.GetComponent<Text>().enabled = true;
			urlImage.enabled = true;
			StartCoroutine (GetFileCo ());
		}
	}
}
