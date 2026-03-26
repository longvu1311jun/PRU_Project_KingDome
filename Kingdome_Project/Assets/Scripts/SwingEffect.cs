using UnityEngine;

public class SwingEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.4f; // match your animation length

    void Start()
    {
        Destroy(gameObject, duration);
    }
}
