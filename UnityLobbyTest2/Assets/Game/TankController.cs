using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TankController : NetworkBehaviour
{
    [SerializeField] Transform cannon;
    [SerializeField] float speed;
    [SerializeField] KeyCode shootKey = KeyCode.Space;

    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform spawnPoint;
    [SyncVar] Vector3 color;
    [SerializeField] List<MeshRenderer> renderers;
    private bool isColorized;

    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            color = new Vector3(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        }
    }

    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            //Movement
            if (Input.GetButton("Horizontal"))
            {
                float value = Input.GetAxis("Horizontal");
                transform.position += new Vector3(value, 0,0 ) * speed * Time.deltaTime;
                transform.forward = Vector3.right * value;
            }
            else if (Input.GetButton("Vertical"))
            {
                float value = Input.GetAxis("Vertical");
                transform.position += new Vector3(0,0, value) * speed * Time.deltaTime;
                transform.forward = Vector3.forward * value;

            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!isColorized && color != Vector3.zero)
        {
            Color newcolor = new Color(color.x, color.y, color.z);
            foreach (var item in renderers)
            {
                item.material.color = newcolor;
            }
            isColorized = true;
        }
        //Debug.LogError(color);
        if (isLocalPlayer)
        {
            // shoot
            if (Input.GetKeyDown(shootKey))
            {
                CmdFire();
            }

            RotateTurret();
        }

    }
    void RotateTurret()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            Debug.DrawLine(ray.origin, hit.point);
            Vector3 lookRotation = new Vector3(hit.point.x, cannon.transform.position.y, hit.point.z);
            cannon.transform.LookAt(lookRotation);
        }
    }

    [Command]
    void CmdFire()
    {
        GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
        NetworkServer.Spawn(projectile);
    }


}
