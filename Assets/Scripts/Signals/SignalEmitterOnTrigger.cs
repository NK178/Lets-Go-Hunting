using UnityEngine;

public class SignalEmitterOnTrigger : MonoBehaviour
{

    [SerializeField] private string[] validTags;

    [SerializeField] private bool isReuseable; 
    [SerializeField] private GameSignal signal; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

        foreach (var tag in validTags)
        {
            if (other.tag == tag)
            {
                EmitSignal();
                return;
            }
        } 
    }

    private void EmitSignal()
    {
        signal.Raise();

        if (!isReuseable)
            gameObject.SetActive(false);
    }
}
