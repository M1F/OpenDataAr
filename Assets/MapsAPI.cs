using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEditor;
using LitJson;
using System.IO;
using System;
using System.Net;
using UnityEngine.SceneManagement;

public class MapsAPI : MonoBehaviour {

    public GameObject originalTable;
    public GameObject[] tables;
    public GameObject texterror;
    public float lat;
    public float lon;
    public GameObject maincam;
	public GameObject RUN;
    public GameObject Menu;
    public Dropdown menuContent;
    public GameObject testTxt;
    public GameObject radar;

    float speed = 5.0f;
    string param = Interactable.Filter;
    Transform target;
    public void MenuInvoker()
    {
        if (Menu.GetComponent<Interactable>().ChosenOptionInt == 2)
        {
            menuContent.options.Clear();
            foreach(Base onebase in Menu.GetComponent<ODCSV>().GetBases())
            {
                menuContent.options.Add(new Dropdown.OptionData() { text = onebase.baseDesc });
            }
            //StartCoroutine(GoGo2());
        }
    }

    public void StartGogGo1()
    {
        StartCoroutine(GoGo1());
    }

    public void StartGogGo2()
    {
        menuContent.options.Clear();
        Menu.GetComponent<ODCSV>().fillBases();
        List<Base> newbase = Menu.GetComponent<ODCSV>().GetBases();
        foreach (Base onebase in newbase)
        {
            menuContent.options.Add(new Dropdown.OptionData() { text = onebase.baseDesc });
        }
        menuContent.RefreshShownValue();
        //StartCoroutine(GoGo2());
    }

    public void ShowGoGo2()
    {
        StartCoroutine(GoGo2());
    }

    IEnumerator GoGo1 () {
        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            print("Time out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            lat = Input.location.lastData.latitude;
            lon = Input.location.lastData.longitude;
         }
        Input.location.Stop();

        //lat = 54.984322F;
        //lon = 82.858975F;
        string url = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location="+lat.ToString()+","+lon.ToString()+ "&radius=500&type=restaurant&key=AIzaSyBdHiBxdXr5M-DKeAP494aTcEUa9imgrPw";
        //string url = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=59.931167,30.360708&radius=500&" +param+"&key=AIzaSyBdHiBxdXr5M-DKeAP494aTcEUa9imgrPw";
        WWW www = new WWW(url);
        yield return www;
        if (www.error == null)
        {
            Processjson(www.text);
        }
        else
        {
            texterror.GetComponent<Text>().text = "ERROR: " + www.error;
        }
    }

    IEnumerator GoGo2()
    {
        Input.location.Start();
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            print("Time out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            lat = Input.location.lastData.latitude;
            lon = Input.location.lastData.longitude;
        }
        Input.location.Stop();
        Menu.GetComponent<ODCSV>().SetCurrBase(menuContent.value);
        List<arObject> neArObject = Menu.GetComponent<ODCSV>().findNearest(82.949649, 55.016980, 1000);
        testTxt.GetComponent<Text>().text = neArObject.Count.ToString();
        if (neArObject != null)
        {
            Process1(neArObject);
        }
        else
        {
        }
        yield return neArObject;
    }

    private void Processjson(string jsonString)
    {
        int rad = 6372795;
        float llat1;
        float llong1;
        float llat2;
        float llong2;
        Debug.Log(jsonString);
        JsonData jsonvale = JsonMapper.ToObject(jsonString);
        tables = new GameObject[jsonvale["results"].Count];
        for (int i = 0; i < jsonvale["results"].Count; i++)
        {
            //Debug.Log(jsonvale["results"][i]["geometry"]["location"]["lat"].ToString());
            //Debug.Log(jsonvale["results"][i]["geometry"]["location"]["lng"].ToString());
            llat1 = lat;
            llong1 = lon;
            //llat1 = 54.984322F;
            //llong1 = 82.858975F;
            llat2 = float.Parse(jsonvale["results"][i]["geometry"]["location"]["lat"].ToString());
            llong2 = float.Parse(jsonvale["results"][i]["geometry"]["location"]["lng"].ToString());

            float lat1 = llat1 * (float)Math.PI / 180;
            float lat2 = llat2 * (float)Math.PI / 180;
            float long1 = llong1 * (float)Math.PI / 180;
            float long2 = llong2 * (float)Math.PI / 180;

            float cl1 = (float)Math.Cos(lat1);
            float cl2 = (float)Math.Cos(lat2);
            float sl1 = (float)Math.Sin(lat1);
            float sl2 = (float)Math.Sin(lat2);
            float delta = long2 - long1;
            float cdelta = (float)Math.Cos(delta);
            float sdelta = (float)Math.Sin(delta);

            float x = (cl1 * sl2) - (sl1 * cl2 * cdelta);
            float y = sdelta * cl2;
            float z = (float)Math.Atan(-y / x)*(180/(float)Math.PI);
            if (x < 0) {
                z = z + 180;
                    }

            float z2 = (z + 180) % 360 - 180;
            z2 = -z2 / (180 / (float)Math.PI);
            float anglerad2 = z2 - ((2 * (float)Math.PI) * (float)Math.Floor((z2 / (2 * (float)Math.PI))));
            float angledeg = (anglerad2 * 180) / (float)Math.PI;

            y = (float)Math.Sqrt(Math.Pow(cl2 * sdelta, 2) + Math.Pow(cl1 * sl2 - sl1 * cl2 * cdelta, 2));
            x = sl1 * sl2 + cl1 * cl2 * cdelta;
            float ad = (float)Math.Atan2(y, x);
            float dist = ad * rad;

            float tablex = maincam.transform.position.x + dist / 60 * (float)Math.Cos(angledeg);
            float tablez = maincam.transform.position.z + dist / 60 * (float)Math.Sin(angledeg);
			           
            tables[i] = Instantiate(originalTable, new Vector3(tablex, 1.96f, tablez), Quaternion.identity);
            tables[i].transform.LookAt(maincam.transform);
            Transform[] ts = tables[i].transform.GetComponentsInChildren<Transform>(true);
            Material FoodMat = Resources.Load("Materials/Fastfood", typeof(Material)) as Material;
			Material opened = Resources.Load("Materials/opened", typeof(Material)) as Material;
			Material closed = Resources.Load("Materials/closed", typeof(Material)) as Material;//getting material
			Material hz = Resources.Load("Materials/hz", typeof(Material)) as Material;
            foreach (Transform t in ts) {
                if (t.gameObject.name == "Caption")
                {
                    t.gameObject.GetComponent<Text>().text = jsonvale["results"][i]["name"].ToString();
                }
                if (t.gameObject.name == "Type")
                {
					t.gameObject.GetComponent<Text>().text = dist.ToString()+" метра";
                }
                if (t.gameObject.name == "Icon")
                {
                    t.gameObject.GetComponent<RawImage>().material = FoodMat; //applying material
                }
				if (t.gameObject.name == "Status")
				{
					try 
					{
						if (jsonvale ["results"] [i] ["opening_hours"]["open_now"].ToString () == "True") {
							GameObject.Find ("Status").GetComponent<Renderer>().material = opened;
							GameObject.Find ("StatusText").GetComponent<Text>().text = "Открыто";
						} 
						else
						{
							GameObject.Find ("Status").GetComponent<Renderer> ().material = closed;
							GameObject.Find ("StatusText").GetComponent<Text>().text = "Закрыто";
						}
					}
					catch
					{
						GameObject.Find ("Status").GetComponent<Renderer> ().material = hz;
						GameObject.Find ("StatusText").GetComponent<Text>().text = "Нет данных";
					}
				}
            };
        }
    }

    private void Process1(List<arObject> data)
    {
        int rad = 6372795;
        float llat1;
        float llong1;
        float llat2;
        float llong2;
        tables = new GameObject[data.Count];
        int i = 0;
        foreach (arObject oneobject in data)
        {
            //oneobject.objProp[3].value
            //Debug.Log(jsonvale["results"][i]["geometry"]["location"]["lat"].ToString());
            //Debug.Log(jsonvale["results"][i]["geometry"]["location"]["lng"].ToString());
            llat1 = lat;
            llong1 = lon;
            llat1 = (float)55.016980;
            llong1 = (float)82.949649;
            llat2 = (float)oneobject.objB;
            llong2 = (float)oneobject.objL;

            float lat1 = llat1 * (float)Math.PI / 180;
            float lat2 = llat2 * (float)Math.PI / 180;
            float long1 = llong1 * (float)Math.PI / 180;
            float long2 = llong2 * (float)Math.PI / 180;

            float cl1 = (float)Math.Cos(lat1);
            float cl2 = (float)Math.Cos(lat2);
            float sl1 = (float)Math.Sin(lat1);
            float sl2 = (float)Math.Sin(lat2);
            float delta = long2 - long1;
            float cdelta = (float)Math.Cos(delta);
            float sdelta = (float)Math.Sin(delta);

            float x = (cl1 * sl2) - (sl1 * cl2 * cdelta);
            float y = sdelta * cl2;
            float z = (float)Math.Atan(-y / x) * (180 / (float)Math.PI);
            if (x < 0)
            {
                z = z + 180;
            }

            float z2 = (z + 180) % 360 - 180;
            z2 = -z2 / (180 / (float)Math.PI);
            float anglerad2 = z2 - ((2 * (float)Math.PI) * (float)Math.Floor((z2 / (2 * (float)Math.PI))));
            float angledeg = (anglerad2 * 180) / (float)Math.PI;

            y = (float)Math.Sqrt(Math.Pow(cl2 * sdelta, 2) + Math.Pow(cl1 * sl2 - sl1 * cl2 * cdelta, 2));
            x = sl1 * sl2 + cl1 * cl2 * cdelta;
            float ad = (float)Math.Atan2(y, x);
            float dist = ad * rad;

            float tablex = maincam.transform.position.x + dist / 60 * (float)Math.Cos(angledeg);
            float tablez = maincam.transform.position.z + dist / 60 * (float)Math.Sin(angledeg);

            tables[i] = Instantiate(originalTable, new Vector3(tablex, 1.96f, tablez), Quaternion.identity);
            tables[i].transform.LookAt(maincam.transform);
            Transform[] ts = tables[i].transform.GetComponentsInChildren<Transform>(true);
            Material FoodMat = Resources.Load("Materials/Fastfood", typeof(Material)) as Material;
            Material opened = Resources.Load("Materials/opened", typeof(Material)) as Material;
            Material closed = Resources.Load("Materials/closed", typeof(Material)) as Material;//getting material
            Material hz = Resources.Load("Materials/hz", typeof(Material)) as Material;
            foreach (Transform t in ts)
            {
                if (t.gameObject.name == "Caption")
                {
                    t.gameObject.GetComponent<Text>().text = oneobject.objProp[0].value.ToString();
                }
                if (t.gameObject.name == "Type")
                {
                    t.gameObject.GetComponent<Text>().text = dist.ToString() + " метра";
                }
                if (t.gameObject.name == "Icon")
                {
                    t.gameObject.GetComponent<RawImage>().material = FoodMat; //applying material
                }
            };
            i++;
        }
    }


    void Update () {
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
        }
        else
        {
            lat = Input.location.lastData.latitude;
            lon = Input.location.lastData.longitude;
        }

        if (Input.GetMouseButton(0))
        {
            transform.LookAt(maincam.transform);
            transform.RotateAround(maincam.transform.position, Vector3.up, Input.GetAxis("Mouse X") * speed);
        }
    }

	public void RunVasya()
	{
		for(int i=0; i < tables.Length; i++){
		Transform[] ts = tables[i].transform.GetComponentsInChildren<Transform>(true);
		foreach (Transform t in ts) {
			t.gameObject.SetActive (false);
		}
		}
		RUN.SetActive (true);
	}
}
