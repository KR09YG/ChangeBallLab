using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class BallCreater : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpinSettings _spinSettings;
    [SerializeField] private PitcherBuild _pitcherBuild;
    [SerializeField] private ParameterSetter _parameterSetter;
    [SerializeField] private DefautlBall _defaultBall;
    [SerializeField] private PitchItemView _ballButton;
    [SerializeField] private Transform _ballButtonParent;

    private int _currentBallId = -1;
    private Button _currentButton;
    private PitchItemView _currentSelectedItem;

    // BallDataだけDictionaryで管理
    private Dictionary<int, BallData> _ballDataDict = new Dictionary<int, BallData>();

    private List<PitchItemView> _buttonPool = new List<PitchItemView>();

    public bool HasBallData => _currentBallId >= 0;

    /// <summary>
    /// 新しいボールを作成
    /// </summary>
    public void CreateNewBall()
    {
        // pointが足りているかをここでチェック

        _currentBallId++;
        BallData newBallData = _defaultBall.Data;

        _ballDataDict.Add(_currentBallId, newBallData);

        Button button = CreateButton();
        PitchItemView itemView = button.GetComponent<PitchItemView>();
        itemView.Initialize(this, _currentBallId);
        itemView.SetBallData(newBallData);

        button.name = newBallData.Name;
        ButtonMark(button);
        button.GetComponentInChildren<TextMeshProUGUI>().text = newBallData.Name;

        _currentSelectedItem = itemView;
        _parameterSetter.SetParameter(newBallData);
    }

    /// <summary>
    /// ボタン選択時
    /// </summary>
    public void ButtonSelect(PitchItemView item)
    {
        ButtonMark(item.Button);
        _currentBallId = item.BallId;
        _currentSelectedItem = item;
        _parameterSetter.SetParameter(_ballDataDict[_currentBallId]);
    }

    /// <summary>
    /// ボタンの選択マークを更新
    /// </summary>
    private void ButtonMark(Button button)
    {
        if (_currentButton)
        {
            _currentButton.image.color = Color.white;
        }

        _currentButton = button;
        _currentButton.image.color = Color.red;
    }

    /// <summary>
    /// ボールデータを設定（更新）
    /// </summary>
    public void SetBall()
    {
        if (_currentBallId < 0)
        {
            Debug.LogWarning("選択中のボールがありません。");
            return;
        }

        BallData updatedData = _parameterSetter.GetParameter();

        // Dictionaryのデータを更新
        _ballDataDict[_currentBallId] = updatedData;

        // ビューを更新
        _currentSelectedItem.SetBallData(updatedData);

        Debug.Log($"ボールID {_currentBallId} のデータを更新しました。");
    }

    /// <summary>
    /// ボールを削除
    /// </summary>
    public void DeleteBall()
    {
        if (_ballDataDict.Count == 0)
        {
            Debug.Log("削除するボールがありません。");
            return;
        }

        if (_currentBallId < 0)
        {
            Debug.LogWarning("選択中のボールがありません。");
            return;
        }

        // Dictionaryから削除
        _ballDataDict.Remove(_currentBallId);

        // ビューをプールに戻す
        _currentSelectedItem.gameObject.SetActive(false);
        _buttonPool.Add(_currentSelectedItem);

        // 次に選択するボールを決定
        if (_ballDataDict.Count > 0)
        {
            // 適当に最初のボールを選択
            int nextBallId = _ballDataDict.Keys.First();

            // 対応するビューを探す
            PitchItemView nextView = FindViewByBallId(nextBallId);
            if (nextView != null)
            {
                ButtonSelect(nextView);
            }
        }
        else
        {
            _currentBallId = -1;
            _currentSelectedItem = null;
            _currentButton = null;
        }
    }

    /// <summary>
    /// BallIdから対応するビューを探す
    /// </summary>
    private PitchItemView FindViewByBallId(int ballId)
    {
        PitchItemView[] allViews = _ballButtonParent.GetComponentsInChildren<PitchItemView>(false);

        foreach (var view in allViews)
        {
            if (view.BallId == ballId)
            {
                return view;
            }
        }

        return null;
    }

    /// <summary>
    /// ボタンを作成または再利用
    /// </summary>
    private Button CreateButton()
    {
        Button button;

        if (_buttonPool.Count == 0)
        {
            button = Instantiate(_ballButton.gameObject, _ballButtonParent).GetComponent<Button>();
        }
        else
        {
            PitchItemView pooledItem = _buttonPool[0];
            button = pooledItem.Button;
            button.transform.SetParent(_ballButtonParent);
            _buttonPool.RemoveAt(0);
            button.gameObject.SetActive(true);
        }

        return button;
    }

    /// <summary>
    /// 現在選択中のボールデータを取得
    /// </summary>
    public BallData GetCurrentBallData()
    {
        if (_currentBallId >= 0 && _ballDataDict.ContainsKey(_currentBallId))
        {
            return _ballDataDict[_currentBallId];
        }

        return default;
    }

    /// <summary>
    /// 全ボールデータをListで取得（ピッチャーに渡す用）
    /// </summary>
    public List<BallData> GetAllBallDataAsList()
    {
        return new List<BallData>(_ballDataDict.Values);
    }

    /// <summary>
    /// ピッチャーに持ち球を設定
    /// </summary>
    public void ApplyToPitcher()
    {
        if (_pitcherBuild != null)
        {
            List<BallData> ballList = GetAllBallDataAsList();
            Debug.Log($"{ballList.Count}個のボールをピッチャーに設定しました。");
        }
    }
}