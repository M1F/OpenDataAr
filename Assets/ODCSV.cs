using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System;
using System.Globalization;

//работа с CSV файлами
//FillBases (url - опционально) - заносит в массив описание и путь к csv файлам
//GetStatus возвращает текстовое описание статуса класса работы с csv файлами
//GetBases возвращает лист объектов с заполненными описаниями csv файлов
//SetCurrBase - устанавливает индекс выбранного csv файла
//ReadBase - возвращает массив объектов на основе установленного в переменной currBase csv файла
//findNearest - возвращает массив объектов относительно своего местоположения double B,L и радиуса поиска (300 по умолчанию)
public class ODCSV : MonoBehaviour
{

    string Status = "idle";
    int currBase = 11;
    List<Base> bases = new List<Base>();
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void fillBases(string url = "http://maps.nso.ru/232/index.php")
    {
        bases.Clear();
        WebClient w = new WebClient();
        w.Encoding = System.Text.Encoding.UTF8;
        string page = w.DownloadString(url);
        string filter = "<td.*?>(.*?)<\\/td>";
        string filterLink = "<a [^>]*href=(?:'(?<href>.*?)')|(?:\"(?<href>.*?)\")";
        string tempBaseDesc = "";
        string tempBaseLink = "";
        MatchCollection baseStrings = Regex.Matches(page, filter);
        if (baseStrings.Count > 0)
        {
            int i = 0;
            foreach (Match match in baseStrings)
            {
                if (i % 2 == 0)
                {
                    tempBaseDesc = match.Groups[1].ToString();
                }
                else
                {
                    MatchCollection links = Regex.Matches(match.Groups[1].ToString(), filterLink);
                    tempBaseLink = links[0].Groups[1].ToString();
                    bases.Add(new Base() { baseDesc = tempBaseDesc, baseLink = tempBaseLink });
                }
                i += 1;


                //Debug.Log(tempBaseDesc+" "+tempBaseLink);
            }
        }
        Status = "filled";
        //Debug.Log(bases.Count);
    }
    public string GetStatus()
    {
        return Status;
    }

    public List<Base> GetBases()
    {
        if (bases.Count > 0)
        {
            return bases;
        }
        else
        {
            throw new System.Exception("Bases not filled");
        }
        
    }

    public string SetCurrBase(int cb)
    {
        string result;
        try
        {
            currBase = cb;
            result = "Sucess"; 
        }
        catch(Exception e)
        {
            result = "Error" + e.ToString();
        }
        return result;
    }

    public List<arObject> readBase()
    {
        List<arObject> arObjects = new List<arObject>();
        if (currBase != -1)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(bases[currBase].baseLink);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string results = sr.ReadToEnd();
            sr.Close();
            string[] lines = results.Split(new[] { '\n' }, StringSplitOptions.None);
            string[] captions = new string[0];
            string[] parts = new string[0];
            List<arProperty> tempProp = new List<arProperty>();
            int i = 0;
            string tempL = "";
            string tempB = "";
            foreach (string line in lines)
            {
                if (i == 0)
                {
                    captions = line.Split(new[] { ';' }, StringSplitOptions.None);
                    i = 1;
                    //Debug.Log(captions[0] + " " + captions[1]);
                }
                else
                {
                    Array.Clear(parts, 0, parts.Length);
                    parts = line.Split(new[] { ';' }, StringSplitOptions.None);
                    //Debug.Log(parts[0]);
                    //Debug.Log(parts[0]);
                    int j = 0;
                    tempProp.Clear();
                    foreach (string part in parts)
                    {
                        if (part.Contains("POINT"))
                        {
                            string[] space = part.Split(new[] { ' ' }, StringSplitOptions.None);
                            string[] objL = space[0].Split(new[] { '(' }, StringSplitOptions.None);
                            string[] objB = space[1].Split(new[] { ')' }, StringSplitOptions.None);
                            tempL = objL[1];
                            tempB = objB[0];
                        }
                        else
                        {
                            //Debug.Log(part);
                            tempProp.Add(new arProperty { caption = captions[j], value = part });
                            //Debug.Log(tempProp[j].caption + " " + tempProp[j].value);
                            j += 1;
                        }
                    }
                    //Debug.Log(line);
                    //Debug.Log(tempProp[0].caption + " " + tempProp[0].value);
                    i += 1;
                    arObjects.Add(new arObject { objL = double.Parse(tempL), objB = double.Parse(tempB), objProp = new List<arProperty>(tempProp) });
                    //Debug.Log(arObjects[i-2].objL + " " + arObjects[i-2].objB + " " + arObjects[i-2].objProp[0].caption + " " + arObjects[i-2].objProp[0].value);
                }
                if (i > 1)
                {
                    //Debug.Log(arObjects[i - 2].objL + " " + arObjects[i - 2].objB + " " + arObjects[i - 2].objProp[0].caption + " " + arObjects[i - 2].objProp[0].value);
                }
            }
        }
        Debug.Log(arObjects[1].objB + " " + arObjects[1].objL + " " + arObjects[1].objProp[0].value);
        return arObjects;
    }

    public List<arObject> findNearest(double myL, double myB, float radius = 500)
    {
        List<arObject> findArObjects = new List<arObject>();
        List<arObject> arObjectsF = readBase();
        //Debug.Log(arObjectsF[3].objB+" "+arObjectsF[3].objL+" "+arObjectsF[3].objProp[0].value);
        int i = 0;
        foreach (arObject arObj in arObjectsF)
        {
            float R = 6371000; // metres
            double B1 = DegreeToRadian(myB);
            double B2 = DegreeToRadian(arObj.objB);
            double L1 = DegreeToRadian(myL);
            double L2 = DegreeToRadian(arObj.objL);
            double deltaB = B2 - B1;
            double deltaL = L2 - L1;
            double a = Math.Sin(deltaB / 2) * Math.Sin(deltaB / 2) +
                    Math.Cos(B1) * Math.Cos(B2) *
                    Math.Sin(deltaL / 2) * Math.Sin(deltaL / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = R * c;
            if (distance <= radius)
            {
                Debug.Log(myB+" "+arObj.objB+" "+myL+" "+arObj.objL+" "+arObj.objProp[0].value);
                findArObjects.Add(arObj);
            }
            i = i + 1;
            //Debug.Log(i+" "+distance+" "+findArObjects.Count);
        }

        return findArObjects;
    }

    private double DegreeToRadian(double angle)
    {
        return Math.PI * angle / 180.0;
    }
}



public class Base
{
    public string baseDesc { get; set; }
    public string baseLink { get; set; }
}

public class arObject
{
    public double objL { get; set; }
    public double objB { get; set; }
    public List<arProperty> objProp = new List<arProperty>();
}

public class arProperty
{
    public string caption { get; set; }
    public string value { get; set; }
}