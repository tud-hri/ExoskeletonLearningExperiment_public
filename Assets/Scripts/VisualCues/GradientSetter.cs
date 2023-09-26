using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GradientSetter", menuName = "ScriptableObjects/GradientSetter", order = 1)]
public class GradientSetter : ScriptableObject
{
    [SerializeField] private Gradient _colorGradient;

    Texture2D _texture;

    public Texture2D Texture {
        get { return _texture; }
    }

    [SerializeField] int _resolution = 512;
    public int Resolution {
        get { return _resolution; }
    }

    [SerializeField] TextureWrapMode _wrapMode = TextureWrapMode.Clamp;
    [SerializeField] FilterMode _filterMode = FilterMode.Bilinear;

    public void OnEnable()
    {
        if(_texture == null)
            _texture = new Texture2D(_resolution, 1, TextureFormat.ARGB32, false, true);
    }

    public void SetGradientKeyTime(int keyNum, float time)
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] keys = (GradientColorKey[])_colorGradient.colorKeys.Clone();
        keys[keyNum].time = time;

        var alphas = _colorGradient.alphaKeys;
        gradient.SetKeys(keys, alphas);
        
        _colorGradient.colorKeys = keys;
    }
    
    public Color EvaluateGradient(float t) => _colorGradient.Evaluate(t);

    public void Bake()
    {
        if (_texture == null)
            return;

        if (_texture.width != _resolution)
            _texture.Resize(_resolution, 1);

        _texture.wrapMode = _wrapMode;
        _texture.filterMode = _filterMode;
        
        Color[] colors = new Color[_resolution];
        for(int i = 0; i < _resolution; ++i)
        {
            var t = (float)i / _resolution;

            colors[i] = _colorGradient.Evaluate(t);
        }

        _texture.SetPixels(colors);
        _texture.Apply(false);
    }
}
