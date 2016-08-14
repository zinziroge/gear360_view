using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// simple access to json
///
/// var json = JsonNode.Parse(jsonString);
/// string foo = json["hoge"][4].Get<string>();
/// </summary>
public class JsonNode : IEnumerable<JsonNode>, IDisposable
{
    object obj;

    public JsonNode(object obj)
    {
        this.obj = obj;
    }

    public void Dispose()
    {
        obj = null;
    }

    public static JsonNode Parse(string json)
    {
        return new JsonNode(MiniJSON.Json.Deserialize(json));
    }

    public JsonNode this [int i]
    {
        get
        {
            if (obj is IList)
            {
                return new JsonNode(((IList)obj)[i]);
            }
            throw new Exception("Object is not IList : " + obj.GetType().ToString());
        }
    }

    public JsonNode this [string key]
    {
        get
        {
            if (obj is IDictionary)
            {
                return new JsonNode(((IDictionary)obj)[key]);
            }
            throw new Exception("Object is not IDictionary : " + obj.GetType().ToString());
        }
    }

    public int Count
    {
        get
        {
            if (obj is IList)
            {
                return ((IList)obj).Count;
            }
            else if (obj is IDictionary)
            {
                return ((IDictionary)obj).Count;
            }
            else
            {
                return 0;
            }
        }
      
    }

    public T Get<T>()
    {
        return (T)obj;
    }

    public IEnumerator<JsonNode> GetEnumerator()
    {
        if (obj is IList)
        {
            foreach (var o in (IList)obj)
            {
                yield return new JsonNode(o);
            }
        }
        else if (obj is IDictionary)
        {
            foreach (var o in (IDictionary)obj)
            {
                yield return new JsonNode(o);
            }
        }
        else
        {
            yield return null;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
