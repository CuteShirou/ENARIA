using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float lifeTime = 7f;

    private void OnEnable()
    {
        StartCoroutine(DisableAfterDelay());
    }

    private System.Collections.IEnumerator DisableAfterDelay()
    {
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false);
    }
}
