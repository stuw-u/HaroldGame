using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    [SerializeField] Transform visualRoot;
    [SerializeField] ParticleSystem particles;
    [SerializeField] ParticleSystem flash;
    [SerializeField] float spinSpeed;
    [SerializeField] float floatSpeed;
    [SerializeField] float floatOffset;
    [SerializeField] float floatSize;
    [SerializeField] float collectionSpeed = 3f;
    [SerializeField] AudioSource source;

    bool collected = false;
    float collectedTimer = 0;
    Vector3 initPosition;
    Material mat;

    private void Awake ()
    {
        initPosition = visualRoot.localPosition;
        mat = new Material(visualRoot.GetComponent<MeshRenderer>().material);
        visualRoot.GetComponent<MeshRenderer>().material = mat;
    }

    void Update()
    {
        visualRoot.localPosition = initPosition + Vector3.up * (Mathf.Sin(Time.time * floatSpeed) * floatSize + floatOffset);
        visualRoot.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.Self);

        if(collected)
        {
            collectedTimer = Mathf.Clamp01(collectedTimer + Time.deltaTime * collectionSpeed);
            mat.SetFloat("_Blow", (collectedTimer * collectedTimer) * 0.7f);
            visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, initPosition + Vector3.up * floatOffset, collectedTimer);
            if (collectedTimer == 1f)
            {
                particles.Play(true);
                Destroy(particles.gameObject, 1f);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.layer != 9) return;
        if (collected) return;

        if(other.TryGetComponent<WeaponUser>(out var component))
        {
            collected = true;
            flash.Play(true);
            source.Play();
            particles.transform.parent = null;
            flash.transform.parent = null;
            Destroy(flash.gameObject, 1f);

            GameManager.CollectCrate();
            component.GiveNewWeapon(GameManager.GetRandomWeapon());
        }
    }
}
