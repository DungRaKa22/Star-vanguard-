using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private readonly Queue<T> m_Pool = new Queue<T>();
    private readonly System.Collections.Generic.List<T> m_Active = new System.Collections.Generic.List<T>();
    private readonly T m_Prefab;
    private readonly Transform m_Parent;
    private readonly int m_InitialPoolSize;
    private int m_TotalCreated; // Real number of instances ever instantiated by this pool

    public System.Collections.Generic.IReadOnlyList<T> ActiveObjects => m_Active;
    public int ActiveCount => m_Active.Count;
    
    public ObjectPool(T i_Prefab, int i_InitialSize = 10, Transform i_Parent = null)
    {
        m_Prefab = i_Prefab;
        m_InitialPoolSize = i_InitialSize;
        m_Parent = i_Parent;
        
        // Pre-populate pool
        for (int i = 0; i < m_InitialPoolSize; i++)
        {
            CreateNewPooledObject();
        }
    }
    
    private void CreateNewPooledObject()
    {
        var newObj = Object.Instantiate(m_Prefab, m_Parent);
        newObj.gameObject.SetActive(false);
        m_Pool.Enqueue(newObj);
        m_TotalCreated++;
    }
    
    public T Get(Vector3 i_Position, Quaternion i_Rotation)
    {
        T obj;
        
        // If pool is empty, create new object
        if (m_Pool.Count == 0)
        {
            CreateNewPooledObject();
        }
        
        obj = m_Pool.Dequeue();
        m_Active.Add(obj);
        obj.transform.position = i_Position;
        obj.transform.rotation = i_Rotation;
        obj.gameObject.SetActive(true);
        
        // Call poolable interface if implemented
        if (obj is IPoolable poolable)
        {
            poolable.OnPoolGet();
        }
        
        return obj;
    }
    
    public T Get(Vector3 i_Position)
    {
        return Get(i_Position, Quaternion.identity);
    }
    
    public void Return(T i_Object)
    {
        if (i_Object == null) return;
        m_Active.Remove(i_Object);
        
        // Call poolable interface if implemented
        if (i_Object is IPoolable poolable)
        {
            poolable.OnPoolReturn();
        }
        
        i_Object.gameObject.SetActive(false);
        m_Pool.Enqueue(i_Object);
    }

    public void ReturnAllActive()
    {
        for (int i = m_Active.Count - 1; i >= 0; i--)
        {
            Return(m_Active[i]);
        }
    }
    
    public int PoolSize => m_Pool.Count;
    public int TotalCreated => m_TotalCreated;
}
