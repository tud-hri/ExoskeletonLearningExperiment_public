using UnityEngine;
using UXF;

public class ColorTracker : Tracker
{
    [SerializeField] private Renderer rend;

    protected override void SetupDescriptorAndHeader()
    {
        Debug.Log("TEst");
        measurementDescriptor = "color";
        
        customHeader = new string[]
        {
            "color"
        };
    }

    protected override UXFDataRow GetCurrentValues()
    {
        var values = new UXFDataRow()
        {
            ("color", rend.material.color)
        };

        return values;
    }
}