using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Personnage : MonoBehaviour
{
    [Header("Actions")]
    public InputAction actionMarche;
    public InputAction actionSaut;
    public InputAction actionTir;
    
    // AJOUTER UNE ACTION POUR LE DASH 👇
    public InputAction actionDash;

    [Header("Déplacement horizontal")]
    float inputDeplacement;
    public float vitesseDeplacement;


    [Header("Saut")]
    bool inputSaut;
    public float forceSaut;
    public bool estAuSol;
    public float direction = 1;

    public LayerMask coucheSol;

    [Header("Tir")]
    bool inputTir;
    public GameObject prefabProjectile;
    public Transform prefabPosition;

    public float timerTir = 0;
    public float timerTirMax = 2;

    [Header("Dash")]
    // AJOUTER LES VARIABLES NÉCESSAIRES POUR LE DASH 👇
    bool inputDash;
    public float forceDash;
    


    [Header("Sons")]
    public AudioClip sonSaut;
    public AudioClip sonTir;

    [Header("Composants")]
    Rigidbody2D rb;
    AudioSource audioSource;

    public Animator animator; // Animator du visuel enfant - Est déjà glissé dans l'inspecteur
    public SpriteRenderer sr; // SpriteRenderer du visuel enfant - Est déjà glissé dans l'inspecteur


    //Active les actions
    private void OnEnable()
    {
        actionMarche.Enable();
        actionSaut.Enable();
        actionTir.Enable();

        //ACTIVER L'ACTION DU DASH 👇
        actionDash.Enable();
    }


    //Désactive les actions
    private void OnDisable()
    {
        actionMarche.Disable();
        actionSaut.Disable();
        actionTir.Disable();

        //DÉSACTIVER L'ACTION DU DASH 👇
        actionDash.Disable();
    }


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        //Lecture et stockage des valeurs des touches
        inputDeplacement = actionMarche.ReadValue<float>();
        inputSaut = actionSaut.WasPressedThisFrame();
        inputTir = actionTir.WasPressedThisFrame();
        //DÉTECTER LA TOUCHE POUR LE DASH ICI 👇
        inputDash = actionDash.WasPressedThisFrame();

        //Vérification du sol
        estAuSol = Physics2D.Raycast(transform.position, Vector2.down, 0.4f, coucheSol);
        Debug.DrawRay(transform.position, Vector3.down * 0.4f, Color.orange);

        //Démarrage de l'animation de course et de saut
        animator.SetFloat("vitesse", Mathf.Abs(rb.linearVelocityX));
        animator.SetBool("estEnSaut", estAuSol == false);

        // GÉRER L'ANIMATION ASSOCIÉE AU DASH ICI 👇 
      

        //Ajustement du sens du personnage en fonction des touches
        //Ajustement du sens du point d'instanciation du projectile
        if (inputDeplacement < 0)
        {
            direction = -1;
            sr.flipX = true;

            Vector2 positionProjectile = prefabPosition.localPosition;
            positionProjectile.x = -1;
            prefabPosition.localPosition = positionProjectile;

        }
        else if (inputDeplacement > 0)
        {
            direction = 1;
            sr.flipX = false;

            Vector2 positionProjectile = prefabPosition.localPosition;
            positionProjectile.x = 1;
            prefabPosition.localPosition = positionProjectile;
        }


        //Gestion du tir si la minuterie est terminée
        if (inputTir && timerTir <= 0)
        {
            timerTir = timerTirMax;

            GameObject clone = Instantiate(prefabProjectile, prefabPosition.position, prefabPosition.rotation);
            clone.GetComponent<Projectile>().direction = direction;
            clone.SetActive(true);

            //Son de tir
            audioSource.PlayOneShot(sonTir);

            //Déclenchement de l'animation du tir
            animator.SetTrigger("tir");
        }

        if (timerTir > 0)
        {
            timerTir -= Time.deltaTime;
            timerTir = Mathf.Max(timerTir, 0); // Empêche d'être plus petit que 0
        }


    }


    // Projectile
    private void FixedUpdate()
    {
        //Changement de la vitesse de déplacement
        if (inputDeplacement != 0)
        {
            rb.linearVelocityX = inputDeplacement * vitesseDeplacement;
        }

        //Ajout de force pour le saut
        if (inputSaut && estAuSol)
        {
            rb.AddForce(Vector2.up * forceSaut, ForceMode2D.Impulse);
            audioSource.PlayOneShot(sonSaut);
        }

        // GÉRER LA PHYSIQUE ASSOCIÉE AU DASH ICI 👇
        if (inputDash)
        {
            rb.AddForce(Vector2.right * forceDash * direction, ForceMode2D.Impulse);
              animator.SetTrigger("Dash");
        }
    }



    //Redémarrage si on touche la lave
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Lave")
        {
            Scene sceneActuelle = SceneManager.GetActiveScene();
            SceneManager.LoadScene(sceneActuelle.name);
        }
    }
}
