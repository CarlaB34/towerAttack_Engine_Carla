using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{ 
    public GameObject prefabToInstantiate;

    public GameObject prefabEnemy;

    public GameObject globalTarget;

    private Camera m_CurrentCamera;
    public void Awake()
    {
        //SoundManager.Instance.Display();
        m_CurrentCamera = FindObjectOfType<Camera>();
    }
    public void Update()
    {

        IntantiateEnemy();
    }

    private void IntantiateEnemy()
    {
        // Creation d'un Ray à partir de la camera
        Ray ray = m_CurrentCamera.ScreenPointToRay(Input.mousePosition);
        float mult = 1000;
        Debug.DrawRay(ray.origin, ray.direction * mult, Color.green);

        // Recuperation du bouton droit de la souris.
        // Method static de la classe Input - GetButtonDown(0) => bouton gauche de la souris
        if (Input.GetMouseButtonDown(0))
        {
            // Method static de la classe Physics - Raycast
            if (Physics.Raycast(ray, out RaycastHit hit, mult, LayerMask.GetMask("Default")))
            {
                // On recupère un élement depuis le poolmanager
                GameObject instantiated = PoolingManager.Instance.GetElement(prefabToInstantiate);
                instantiated.transform.position = hit.point;
                instantiated.SetActive(true);

                Entity entity = instantiated.GetComponent<Entity>();
                if (entity)
                {
                    entity.InitEntity();
                    if (entity is EntityMoveable moveable)
                    {
                        moveable.SetGlobalTarget(globalTarget);
                    }
                    entity.RestartEntity();
                }
            }
        }
        //clicl droit pour enemy
        if (Input.GetMouseButtonDown(1))
        {
            // 
            if (Physics.Raycast(ray, out RaycastHit hit, mult, LayerMask.GetMask("Default")))
            {
                // On recupère un élement depuis le poolmanager
                GameObject instantiated = PoolingManager.Instance.GetElement(prefabEnemy);
                instantiated.transform.position = hit.point;
                instantiated.SetActive(true);
            }
        }
    }
}



