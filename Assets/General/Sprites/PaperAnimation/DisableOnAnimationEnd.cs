using UnityEngine;

public class DisableOnAnimationEnd : MonoBehaviour
{
    // Метод для отключения GameObject
    public void DisableGameObject()
    {
        gameObject.SetActive(false);
    }
}
