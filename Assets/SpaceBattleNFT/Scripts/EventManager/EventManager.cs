using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    public bool LimitQueueProcesing;
    public float QueueProcessTime;
    private static EventManager _instance;
    
    private readonly Queue _eventQueue = new Queue();

    public delegate void EventDelegate<in T>(T e) where T : GameEvent;

    private delegate void EventDelegate(GameEvent e);

    private readonly Dictionary<System.Type, EventDelegate> _delegates = new Dictionary<System.Type, EventDelegate>();

    private readonly Dictionary<System.Delegate, EventDelegate>
        _delegateLookup = new Dictionary<System.Delegate, EventDelegate>();

    private readonly Dictionary<System.Delegate, System.Delegate> _onceLookups =
        new Dictionary<System.Delegate, System.Delegate>();

    public static EventManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(EventManager)) as EventManager;
            }

            return _instance;
        }
    }

    private EventDelegate AddDelegate<T>(EventDelegate<T> del) where T : GameEvent
    {
        // Early-out if we've already registered this delegate
        if (_delegateLookup.ContainsKey(del))
            return null;

        // Create a new non-generic delegate which calls our generic one.
        // This is the delegate we actually invoke.
        void InternalDelegate(GameEvent e) => del((T)e);
        _delegateLookup[del] = InternalDelegate;

        EventDelegate tempDel;
        if (_delegates.TryGetValue(typeof(T), out tempDel))
        {
            _delegates[typeof(T)] = tempDel += InternalDelegate;
        }
        else
        {
            _delegates[typeof(T)] = InternalDelegate;
        }

        return InternalDelegate;
    }

    public void AddListener<T>(EventDelegate<T> del) where T : GameEvent
    {
        AddDelegate<T>(del);
    }

    public void AddListenerOnce<T>(EventDelegate<T> del) where T : GameEvent
    {
        EventDelegate result = AddDelegate<T>(del);

        if (result != null)
        {
            // remember this is only called once
            _onceLookups[result] = del;
        }
    }

    public void RemoveListener<T>(EventDelegate<T> del) where T : GameEvent
    {
        EventDelegate internalDelegate;
        if (_delegateLookup.TryGetValue(del, out internalDelegate))
        {
            EventDelegate tempDel;
            if (_delegates.TryGetValue(typeof(T), out tempDel))
            {
                tempDel -= internalDelegate;
                if (tempDel == null)
                {
                    _delegates.Remove(typeof(T));
                }
                else
                {
                    _delegates[typeof(T)] = tempDel;
                }
            }

            _delegateLookup.Remove(del);
        }
    }

    public void RemoveAll()
    {
        _delegates.Clear();
        _delegateLookup.Clear();
        _onceLookups.Clear();
    }

    public bool HasListener<T>(EventDelegate<T> del) where T : GameEvent
    {
        return _delegateLookup.ContainsKey(del);
    }

    public void TriggerEvent(GameEvent e)
    {
        EventDelegate del;
        if (_delegates.TryGetValue(e.GetType(), out del))
        {
            del.Invoke(e);

            // remove listeners which should only be called once
            foreach (EventDelegate k in _delegates[e.GetType()].GetInvocationList())
            {
                if (_onceLookups.ContainsKey(k))
                {
                    _delegates[e.GetType()] -= k;

                    if (_delegates[e.GetType()] == null)
                    {
                        _delegates.Remove(e.GetType());
                    }

                    _delegateLookup.Remove(_onceLookups[k]);
                    _onceLookups.Remove(k);
                }
            }
        }
        else
        {
            Debug.LogWarning("Event: " + e.GetType() + " has no listeners");
        }
    }

    //Inserts the event into the current queue.
    public bool QueueEvent(GameEvent evt)
    {
        if (!_delegates.ContainsKey(evt.GetType()))
        {
            Debug.LogWarning("EventManager: QueueEvent failed due to no listeners for event: " + evt.GetType());
            return false;
        }

        _eventQueue.Enqueue(evt);
        return true;
    }

    private void Update()
    {
        float timer = 0.0f;
        while (_eventQueue.Count > 0)
        {
            if (LimitQueueProcesing)
            {
                if (timer > QueueProcessTime)
                    return;
            }

            GameEvent evt = _eventQueue.Dequeue() as GameEvent;
            TriggerEvent(evt);

            if (LimitQueueProcesing)
                timer += Time.deltaTime;
        }
    }

    public void OnDestroy()
    {
        RemoveAll();
        _eventQueue.Clear();
        _instance = null;
    }
}