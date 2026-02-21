using UnityEngine;
using System.Collections;

public class PlayerKnockback : MonoBehaviour
{
    private CharacterController _characterController;
    private bool isKnockedBack = false;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackUpwardForce = 2f;
    [SerializeField] private float knockbackDuration = 0.3f;

    //　プレイヤーがノックバック中かどうかを外部から確認できるようにするプロパティ
    public bool IsKnockedBack => isKnockedBack;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    public void ApplyKnockback(Vector3 direction)
    {
        if (!isKnockedBack)
        {
            StartCoroutine(KnockbackCoroutine(direction));
        }
    }

    private IEnumerator KnockbackCoroutine(Vector3 direction)
    {
        isKnockedBack = true;
        float elapsedTime = 0f;

        // ノックバックの初期速度を計算
        Vector3 knockbackVelocity = direction.normalized * knockbackForce + Vector3.up * knockbackUpwardForce;

        while (elapsedTime < knockbackDuration)
        {
            float t = elapsedTime / knockbackDuration;
            Vector3 currentVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, t);
            // ノックバックの移動を適用
            _characterController.Move(currentVelocity * Time.deltaTime);

            // 経過時間を更新
            elapsedTime += Time.deltaTime;

            yield return null; // 次のフレームまで待機
        }

        isKnockedBack = false;
    }
}
