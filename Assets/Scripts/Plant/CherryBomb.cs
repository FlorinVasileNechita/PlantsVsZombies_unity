using UnityEngine;
using System.Collections;

public class CherryBomb : MonoBehaviour {

    public AudioClip explodeSound;
    public GameObject effect;
    public Vector3 effectOffset;
    public float explodeRange;
    public float delayTime;

    void AfterGrow() {
        transform.FindChild("plant").GetComponent<Animator>().Rebind();
        StartCoroutine(Explode());
    }

    IEnumerator Explode() {
        yield return new WaitForSeconds(delayTime);

        GameObject newEffect = Instantiate(effect);
        newEffect.transform.position = transform.position + effectOffset;
        newEffect.GetComponent<SpriteRenderer>().sortingOrder =
            transform.FindChild("plant").GetComponent<SpriteRenderer>().sortingOrder + 1;
        Destroy(newEffect, 1.5f);

        SearchZombie search = GetComponent<SearchZombie>();
        foreach (GameObject zombie in search.SearchZombiesInRange(explodeRange)) {
            zombie.GetComponent<ZombieHealthy>().BoomDie();
        }

        AudioManager.GetInstance().PlaySound(explodeSound);
        GetComponent<PlantHealthy>().Die();
    }
}
