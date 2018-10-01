using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR

[ExecuteInEditMode]
[System.Serializable]
public class PatternContainer : MonoBehaviour
{
    [SerializeField] public List<PatternBuilder.Pattern> patterns = new List<PatternBuilder.Pattern>();
    private string reason;
    //faux "constructor", init patterncontainer
    public void Setup(List<PatternBuilder.Pattern> initPatterns){  patterns = initPatterns;    }

    //Removes the currently "selected" pattern, sets iterator back to initial value
    public bool RemovePattern(int index)
    {
        bool failed = false;
        if (patterns.Count > 0 && index <= patterns.Count)
            patterns.Remove(patterns[index]);
        else if(index > patterns.Count){
            failed = true;
            reason = "Index indicated is higher than number of contained elements.";
        }
        else{
            failed = true;
            reason = "No patterns to remove.";
        }
        return failed;
    }
    public bool ClearPatterns()
    {
        bool failed = false;
        if (patterns.Count != 0)
            patterns.Clear();
        else{
            failed = true;
            reason = "No patterns to clear.";
        }
        return failed;
    }
    public string GetReason() { return reason; }
}
#endif