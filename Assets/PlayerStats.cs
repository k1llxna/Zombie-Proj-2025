using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerStats : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Weapon weapon;

    public int score = 0;
    public int maxAmmo;
    public int currentAmmo;
    
    public TMP_Text scoreText;
    public TMP_Text cAmmoText;
    public TMP_Text mAmmoText;
    public TMP_Text reloadText;

    public GameObject gunFlash;
    public Transform flashLoc;

    private bool canShoot = true;
    private bool canReload = true;
    private bool cursorOn = true;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask mask;

    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private float sensitivity = 8f;

    public GameObject gameOverMenu;
    private PlayerMovement motor;

    private float xValue;
    private float zValue;
    private float yRotation;
    private float xRotation;

    private Vector3 horizontal;
    private Vector3 vertical;
    private Vector3 velocity;
    private Vector3 rotation;
    private Vector3 camRotation;

    void Start()
    {
        motor = GetComponent<PlayerMovement>();

        if (maxAmmo == 0)
        {
            maxAmmo = 10;
        }

        currentAmmo = maxAmmo;
        cAmmoText.text = currentAmmo.ToString();
        mAmmoText.text = maxAmmo.ToString();
        scoreText.text = "Points: 0";
        cursorOn = false;

        Cursor.lockState = CursorLockMode.Confined; 
        Cursor.visible = false;
    }

    void Update()
    {
        if (cursorOn == false)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
        xValue = Input.GetAxisRaw("Horizontal");
        zValue = Input.GetAxisRaw("Vertical");

        horizontal = transform.right * xValue;
        vertical = transform.forward * zValue;

        velocity = (horizontal + vertical).normalized * speed;

        motor.Move(velocity);

        // turning
        yRotation = Input.GetAxisRaw("Mouse X");
        rotation = new Vector3(0f, yRotation, 0f) * sensitivity;
        motor.Rotate(rotation);

        // tilting
        xRotation = Input.GetAxisRaw("Mouse Y");
        camRotation = new Vector3(xRotation / 2f, 0f, 0f) * sensitivity;
        //float cameraRotation = xRotation * sensitivity;
        motor.RotateCam(camRotation);

        if (currentAmmo <= 0 || Time.timeScale == 0.0f)
        {
            canShoot = false;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            if (canShoot == true)
            {
                Fire();
            }
        }

        if (Input.GetButtonDown("Fire2"))
        {
            if (canReload == true)
            {
                StartCoroutine(Reload());
            }
        }

        cAmmoText.text = currentAmmo.ToString();
        mAmmoText.text = maxAmmo.ToString();
    }

    void Fire()
    {
        currentAmmo--;
        FindObjectOfType<AudioManager>().Play("Gunshot");
        Instantiate(gunFlash, flashLoc.transform.position, flashLoc.transform.rotation);
        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, weapon.range, mask))
        {
            EnemyController enemy = hit.transform.GetComponent<EnemyController>();
            if (enemy != null)
            {
                //score += enemy.pointValue;
                scoreText.text = "Points: " + score;
                enemy.Die();
            }
        }
    }

    IEnumerator Reload()
    {
        Debug.Log("Reloading");
        canReload = false;
        canShoot = false;
        reloadText.text = "Reloading";
        yield return new WaitForSeconds(3.0f);
        reloadText.text = "";
        currentAmmo = maxAmmo;
        canReload = true;
        canShoot = true;
    }

    void OnCollisionEnter(Collision mob)
    {
        if (mob.gameObject.tag == "Enemy")
        {
            Time.timeScale = 0.0f;
            gameOverMenu.SetActive(true);
            cursorOn = true;
        }
    }
}
