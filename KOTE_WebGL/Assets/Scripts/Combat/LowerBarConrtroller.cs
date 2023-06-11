using DG.Tweening;
using UnityEngine;

public class LowerBarConrtroller : MonoBehaviour
{
    [SerializeField]
    private HealthBarController healthBar;
    [SerializeField]
    private RectTransform healthText;
    [SerializeField]
    private RectTransform DefenseIcon;
    [SerializeField]
    private Transform statusManager;
    
    
    public bool fixedPosition = false;

    // we need to track the entity type because the bar is flipped depending on if it's a player or enemy
    public EntityType attachedEntity;
    public Size size = Size.medium;
    public bool run;
    private bool considerShield;

    private Vector3 healthCenter;
    private Vector3 healthOffset;

    void Start()
    {
        //healthBar.widthAdjustment = -DefenseIcon.sizeDelta.x;
        healthBar.widthAdjustment = 0;

        var defCon = DefenseIcon.GetComponent<DefenseController>();
        if (defCon != null)
        {
            defCon.ShieldRestored.AddListener(RepositionHealth);
            defCon.ShieldBroken.AddListener(CenterHealth);
        }
    }

    private void CenterHealth() 
    {
        considerShield = false;
        healthBar.transform.DOLocalMove(healthCenter, 1).SetEase(Ease.InOutSine);
    }

    private void RepositionHealth()
    {
        considerShield = true;
        healthBar.transform.DOLocalMove(healthOffset, 1).SetEase(Ease.InOutSine);
    }

    private void OnEnable()
    {
        considerShield = false;
        StartCoroutine(Utils.RunAfterTime(() => { run = true; }, 0.1f));
    }

    public void SetSize(Size newSize)
    {
        if (fixedPosition) return;
        healthBar.SetSize(newSize);
        float defenseWidth = DefenseIcon.sizeDelta.x - (DefenseIcon.sizeDelta.x * 0.2f);
        float healthWidth = (healthBar.transform as RectTransform).sizeDelta.x;
        float center = transform.position.x;
        float fullWidth = healthWidth + defenseWidth;
        float leftEdge = center - (fullWidth / 2);
        float rightEdge = center + (fullWidth / 2);

        if (!considerShield)
        {
            healthBar.transform.position = new Vector3(center,
                            healthBar.transform.position.y, healthBar.transform.position.z);

            healthText.transform.position = new Vector3(center,
            healthText.transform.position.y, healthText.transform.position.z);
        }
        healthCenter = healthBar.transform.InverseTransformPoint(new Vector3(center,
                        healthBar.transform.position.y, healthBar.transform.position.z));

        switch (attachedEntity)
        {
            case EntityType.Player:
                DefenseIcon.transform.position = new Vector3(rightEdge - (defenseWidth / 2),
                    DefenseIcon.transform.position.y, DefenseIcon.transform.position.z);

                if (considerShield)
                {
                    healthBar.transform.position = new Vector3(rightEdge - defenseWidth - (healthWidth / 2),
                        healthBar.transform.position.y, healthBar.transform.position.z);

                    healthText.transform.position = new Vector3(rightEdge - defenseWidth - (healthWidth / 2),
                    healthText.transform.position.y, healthText.transform.position.z);
                }

                healthOffset = healthBar.transform.InverseTransformPoint(new Vector3(rightEdge - defenseWidth - (healthWidth / 2),
                healthBar.transform.position.y, healthBar.transform.position.z));
                break;
            case EntityType.Enemy:
                DefenseIcon.transform.position = new Vector3(leftEdge + (defenseWidth / 2),
                    DefenseIcon.transform.position.y, DefenseIcon.transform.position.z);

                if (considerShield)
                {
                    healthBar.transform.position = new Vector3(leftEdge + defenseWidth + (healthWidth / 2),
                        healthBar.transform.position.y, healthBar.transform.position.z);

                    healthText.transform.position = new Vector3(leftEdge + defenseWidth + (healthWidth / 2),
                    healthText.transform.position.y, healthText.transform.position.z);
                }

                healthOffset = healthBar.transform.InverseTransformPoint(new Vector3(leftEdge + defenseWidth + (healthWidth / 2),
                healthBar.transform.position.y, healthBar.transform.position.z));
                break;
        }
        

        statusManager.position = new Vector3(leftEdge + defenseWidth,
            statusManager.position.y, statusManager.position.z);
        size = newSize;
    }

    private void Update()
    {
        if (run) 
        {
            run = false;
            SetSize(size);
        }
    }

}
