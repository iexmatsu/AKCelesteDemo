using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WaapiJsonClassBase<T>
{
    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
    public static implicit operator string(WaapiJsonClassBase<T> obj)
    {
        return obj.ToString();
    }
    public static T Create(string FromJson)
    {
        return JsonUtility.FromJson<T>(FromJson);
    }
}

[System.Serializable]
public class ProjectInfo : WaapiJsonClassBase<ProjectInfo>
{
    [System.Serializable]
    public class ProjectInfoDirectories
    {
        public string authoring;
        public string bin;
        public string help;
        public string install;
        public string log;
        public string user;
    }

    [System.Serializable]
    public class VersionInfo
    {
        public int build;
        public string displayName;
        public int major;
        public int minor;
        public string nickname;
        public int schema;
        public int year;
    }

    public int apiVersion;
    public string branch;
    public string configuration;
    public string copyright;
    public ProjectInfoDirectories directories;
    public string displayName;
    public bool isCommandLine;
    public string platform;
    public int processId;
    public string processPath;
    public VersionInfo version;
}

[System.Serializable]
public class ObjectCreatedJsonObject : WaapiJsonClassBase<ObjectCreatedJsonObject>
{
    [System.Serializable]
    public class subObject
    {
        public string id;
    }
    public subObject @object;
}

[System.Serializable]
public class ObjectGetArgs : WaapiJsonClassBase<ObjectGetArgs>
{
    [System.Serializable]
    public class From
    {
        public string[] id;
    }
    public From from = new From();
}

[System.Serializable]
public class CallOptions : WaapiJsonClassBase<CallOptions>
{
    public string[] @return;
}

[System.Serializable]
public class CallReturn : WaapiJsonClassBase<CallReturn>
{
    [System.Serializable]
    public class ReturnContents
    {
        public string name;
        public string type;
        public string path;
    }
    public ReturnContents[] @return;
}

[System.Serializable]
public class MyArgument
{
    public string host;
}
[System.Serializable]

public class WaapiTest : MonoBehaviour
{
    public static void OnObjectCreated(ulong subID, string Json)
    {
        ObjectCreatedJsonObject CreatedObject = ObjectCreatedJsonObject.Create(Json);
        if (CreatedObject != null)
        {
            string Result;
            var Args = new ObjectGetArgs { from = new ObjectGetArgs.From { id = new[] { CreatedObject.@object.id } } };
            var Options = new CallOptions { @return = new[] { "name", "type", "path" } };
            AkWaapiClient.Call("ak.wwise.core.object.get", Args, Options, out Result);
            var ReturnedObject = CallReturn.Create(Result);
            Debug.Log("New object of type " + ReturnedObject.@return[0].type + " named " + ReturnedObject.@return[0].name + " with ID " + CreatedObject.@object.id + " created at path " + ReturnedObject.@return[0].path);
        }
    }
#if UNITY_EDITOR
    [MenuItem("WWISE/WAAPI/ConnectToWwise %#&c")]
    public static void WaapiClientTest()
    {
        if (AkWaapiClient.Connect("127.0.0.1", 8080))
        {
            Debug.Log("Connect Success!");
            string WaapiResult = string.Empty;

            MyArgument myArgument = new MyArgument();
            myArgument.host = "127.0.0.1";
            string json = JsonUtility.ToJson(myArgument);

            bool res = AkWaapiClient.Call("ak.wwise.core.remote.connect", json, "{}", out WaapiResult);
            //connect wwise from menu
            //https://docs.unity3d.com/Manual/JSONSerialization.html

            if (res)
            {
                var projectInfo = ProjectInfo.Create(WaapiResult);
                Debug.LogWarning(projectInfo);
            }
            else
            {
                Debug.Log("Call failed :(");
            }
            var Options = new CallOptions { @return = new[] { "id" } };
            ulong subId = 0;
            res = AkWaapiClient.Subscribe("ak.wwise.core.object.created", Options, OnObjectCreated, out subId, out WaapiResult);
            if (res)
            {
                Debug.Log("Subscribe success!" + WaapiResult);
            }
            else
            {
                Debug.Log("Subscribe failed :(");
            }
        }
        else
        {
            Debug.Log("Connect fail :(");
        }
    }
    [MenuItem("WWISE/WAAPI/DisconnectFromWwise %#&d")]
    public static void WaapiClientTest2()
    {
        if (AkWaapiClient.Connect("127.0.0.1", 8080))
        {
            Debug.Log("Connect Success!");
            string WaapiResult = string.Empty;
            bool res = AkWaapiClient.Call("ak.wwise.core.remote.disconnect", "{}", "{}", out WaapiResult);
            if (res)
            {
                var projectInfo = ProjectInfo.Create(WaapiResult);
                Debug.LogWarning(projectInfo);
            }
            else
            {
                Debug.Log("Call failed :(");
            }
            var Options = new CallOptions { @return = new[] { "id" } };
            ulong subId = 0;
            res = AkWaapiClient.Subscribe("ak.wwise.core.object.created", Options, OnObjectCreated, out subId, out WaapiResult);
            if (res)
            {
                Debug.Log("Subscribe success!" + WaapiResult);
            }
            else
            {
                Debug.Log("Subscribe failed :(");
            }
        }
        else
        {
            Debug.Log("Connect fail :(");
        }
    }
#endif
}