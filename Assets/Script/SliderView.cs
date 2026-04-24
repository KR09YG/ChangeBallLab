using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderView : MonoBehaviour
{
    [SerializeField] private Vector2 _minMaxValue;
    [SerializeField] private Slider _slider;
    [SerializeField] private TMP_InputField _inputField;

    private void Awake()
    {
        Initialize();

        _slider.onValueChanged.AddListener(OnSliderValueChanged);
        _inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
    }

    private void OnDestroy()
    {
        _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        _inputField.onEndEdit.RemoveListener(OnInputFieldEndEdit);
    }

    private void Initialize()
    {
        _slider.minValue = _minMaxValue.x;
        _slider.maxValue = _minMaxValue.y;
    }

    private void OnSliderValueChanged(float value)
    {
        _inputField.SetTextWithoutNotify(value.ToString("0.0"));
    }

    private void OnInputFieldEndEdit(string text)
    {
        // 入力されたテキストをfloatに変換し、スライダーの値を更新
        // 変換に失敗した場合は、スライダーの現在の値を入力フィールドに表示
        if (float.TryParse(text, out float result))
        {
            result = Mathf.Clamp(result, _minMaxValue.x, _minMaxValue.y);

            _slider.SetValueWithoutNotify(result);
            _inputField.SetTextWithoutNotify(result.ToString("0.0"));
        }
        else
        {
            _inputField.SetTextWithoutNotify(_slider.value.ToString("0.0"));
        }
    }
}