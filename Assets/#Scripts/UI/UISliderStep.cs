using UnityEngine;
using UnityEngine.UI;

public class UISliderStep : MonoBehaviour
{
    [Tooltip("The desired difference between neighbouring values of the Slider component.")]
    [MinAttribute(0.0001f)]
    public float StepSize = 0.0001f;

    private Slider _slider;

    void Start()
    {
        _slider = GetComponent<Slider>();
        if (_slider != null)
        {
            _slider.onValueChanged.AddListener(ClampSliderValue);
        }
    }

    /// <summary>
    /// Calculates the nearest stepped value and updates the Slider component.
    /// </summary>
    /// <param name="value">Current slider value</param>
    public void ClampSliderValue(float value)
    {
        if (_slider != null && StepSize > 0)
        {
            float steppedValue = Mathf.Round(value / StepSize) * StepSize;
            if (steppedValue != value)
            {
                _slider.value = steppedValue;
                Debug.Log(string.Format("New stepped Slider value: {0}", _slider.value));
            }

#if UNITY_EDITOR
            // Help find non-sensical values during development. Gets stripped out for platform build.
            int _numberOfSteps = (int)((_slider.maxValue - _slider.minValue) / StepSize);
            if (_numberOfSteps < 1 || steppedValue < _slider.minValue || steppedValue > _slider.maxValue)
            {
                Debug.LogWarning(string.Format("StepSize is too large. Consider reducing StepSize to less than {0}.", _slider.maxValue - _slider.minValue));
            }
#endif
        }
    }
}
