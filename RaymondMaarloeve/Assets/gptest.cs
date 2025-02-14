using System.Diagnostics;
using UnityEngine;
using LLMUnity;
using Debug = UnityEngine.Debug;

public class gptest : MonoBehaviour
{
    public LLMCharacter llmCharacter;

    private string rep;
    private Stopwatch timer;

    void Start()
    {
        llmCharacter = gameObject.GetComponent<LLMCharacter>();   
        string message = "Hello bot!";
        timer = new Stopwatch();
        timer.Start();
        _ = llmCharacter.Chat(message, HandleReply, ReplyCompleted);
    }
    void HandleReply(string reply){
        this.rep = reply;
    }
    
    void ReplyCompleted(){
        timer.Stop();
        Debug.Log($"The AI replied: {rep}, time: {timer.Elapsed.ToString(@"m\:ss\.fff")}");
    }
}
