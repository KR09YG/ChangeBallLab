using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallSelect : MonoBehaviour
{
    [SerializeField] private PitcherController _pitcherController;
    [SerializeField] private PitcherAnimationController _animationController;
    [SerializeField] private TextMeshProUGUI _ballNameText;
    [SerializeField] private Button[] _selectButtons;
    [SerializeField] private float _animationDuration = 1.0f; // アニメーションの想定時間
    private int _ballIndex = 0;
    private bool _isProcessing = false; // 処理中フラグ

    private void Awake()
    {
        _ballNameText.text = _pitcherController.BallList[_ballIndex].Name;
    }

    public void SelectBall(int direction)
    {
        // 処理中なら無視
        if (_isProcessing)
        {
            Debug.Log("アニメーション再生中のため、入力を無視しました。");
            return;
        }

        SelectBallAsync(direction, this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid SelectBallAsync(int direction, System.Threading.CancellationToken cancellationToken)
    {
        try
        {
            _isProcessing = true; // 処理開始
            SetButtonsInteractable(false);

            _ballIndex += direction;
            int ballCount = _pitcherController.BallList.Count;
            _ballIndex = (_ballIndex + ballCount) % ballCount;

            _ballNameText.text = _pitcherController.BallList[_ballIndex].Name;
            _pitcherController.ChangeBall(_pitcherController.BallList[_ballIndex]);
            _animationController.SetTrigger(PitcherAnimationState.ShakeHead);

            // アニメーション開始を待つ
            await UniTask.Yield(cancellationToken);

            // アニメーション終了を待つ
            await UniTask.WaitForSeconds(_animationDuration);
        }
        catch (System.OperationCanceledException)
        {
            // キャンセル時は無視
            Debug.Log("BallSelect処理がキャンセルされました。");
        }
        finally
        {
            // 必ず実行される後処理
            SetButtonsInteractable(true);
            _isProcessing = false; // 処理終了
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        foreach (var button in _selectButtons)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }
}