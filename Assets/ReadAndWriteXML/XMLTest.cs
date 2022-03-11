using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class XMLTest : MonoBehaviour {


    private ArrayList array = new ArrayList();
	void Start () {
        loadXML();


        //System.Environment.SetEnvironmentVariable("uuu", "uuu", System.EnvironmentVariableTarget.User);

    }
 
	void Update () {
		
	}

    void loadXML()
    {
        XmlDocument xml = new XmlDocument();
        XmlReaderSettings set = new XmlReaderSettings();
        set.IgnoreComments = true;
        xml.Load(XmlReader.Create("C:\\Users\\PDC-48\\Desktop\\ProjectTest\\Assets\\ReadAndWriteXML\\ArthroplastyPlan.vcxproj.user", set));
        //Debug.Log(xml.DocumentElement.SelectNodes("LocalDebuggerEnvironment")[0].InnerText);
        //XmlNodeList xmlNodeList = xml.DocumentElement.SelectNodes("PropertyGroup");
        XmlNodeList xmlNodeList = xml.GetElementsByTagName("LocalDebuggerEnvironment");

        xmlNodeList[0].InnerText = "123";
        foreach (XmlElement item in xmlNodeList)
        {
            Debug.Log(item.InnerText); 
        }

        xml.Save("C:\\Users\\PDC-48\\Desktop\\ProjectTest\\Assets\\ReadAndWriteXML\\myXML.xml");
    }
}
