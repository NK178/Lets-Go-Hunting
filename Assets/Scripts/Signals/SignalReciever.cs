using UnityEngine;
using UnityEngine.Events;

public class SignalReciever : MonoBehaviour
{


    [SerializeField] private GameSignal[] recieveSignals;

    public UnityEvent<GAMESIGNAL> OnSignalRecieved;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        

    }


    private void OnEnable()
    {
        foreach (GameSignal signal in recieveSignals)
        {
            signal.RegisterListener(RespondToSignal);
        }
    }

    private void OnDisable()
    {
        foreach (GameSignal signal in recieveSignals)
        {
            signal.UnregisterListener(RespondToSignal);
        }
    }

    private void RespondToSignal(GAMESIGNAL gamePhase)
    {
        //Debug.Log("RECIEVED SIGNAL");
        OnSignalRecieved?.Invoke(gamePhase);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
