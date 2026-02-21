using UnityEngine;

public class DamageBlock : MonoBehaviour
{
    // 食らうダメージ量
    [SerializeField] private int damageAmount = 1;
    public int DamageAmount => damageAmount;
    // private void OnCollisionEnter(Collision collision)
    // {
    //     // プレイヤーにダメージを与える
    //     PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
    //     if (playerHealth != null)
    //     {
    //         playerHealth.TakeDamage(damageAmount);
    //     }
    // }
    // // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void Start()
    // {
        
    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }
}
