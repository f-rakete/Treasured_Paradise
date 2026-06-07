using UnityEngine;

public class OxygenSystem : MonoBehaviour
{
    public PlayerData PlayerData;

    public float depthMultiplierPer10M = 0.02f;
    public float springDrainBonus = 0.5f;
    public float heavyArtifactBonus = 0.3f;
    public float sirenPanicBonus = 0.8f;
    public float zero;
    public bool isSprinting;
    public bool isCarryingHeavyArtifact;
    public bool isInSirenEncounter;
        
    private float _currentDepth;
    private float _baseDrainRate;
        
    void Start()
    {
        SetTierStatus(PlayerData.CurrentOxygenTier);
    }
    void Update()
    {
        if (_currentDepth <= zero)
        {
            Resurface();
            return;
        }
        DrainOxygen(Time.deltaTime);
        CheckWarningState();
    }
        
    public void SetDepth(float depth) => _currentDepth = depth;

    void DrainOxygen(float deltaTime)
    {
        float multiplier = 1f;
        multiplier += (_currentDepth * 10f) * depthMultiplierPer10M;
        if (isSprinting)
        {
            multiplier *= springDrainBonus;
        }

        if (isCarryingHeavyArtifact)
        {
            multiplier *= heavyArtifactBonus;
        }

        if (isInSirenEncounter)
        {
            multiplier *= sirenPanicBonus;
        }
            
        PlayerData.OxygenCurrent -= _baseDrainRate * multiplier * deltaTime;
        PlayerData.OxygenCurrent = Mathf.Max(zero, PlayerData.OxygenCurrent);

        if (PlayerData.OxygenCurrent <= zero)
        {
            OnOxygenDepeleted?.Invoke(0);
        }
    }

    void CheckWarningState()
    {
        float percent = PlayerData.OxygenCurrent / PlayerData.OxygenMax;
        if (percent >= 0.25f)
        {
            OnCritical?.Invoke(percent);
        }
        else if (percent >= 0.75f)
        {
            OnCaution?.Invoke(percent);
        }
        else
        {
            OnSafe?.Invoke(percent);
        }
    }

    void Resurface()
    {
        PlayerData.OxygenCurrent = PlayerData.OxygenMax;
    }
        
    void SetTierStatus(OxygenTier tier)
    {
        switch (tier)
        {
            case OxygenTier.Snorkel:
                PlayerData.OxygenMax = 30f;
                _baseDrainRate = 1f;
                break;
            case OxygenTier.BasicScuba:
                PlayerData.OxygenMax = 300f;
                _baseDrainRate = 0.8f;
                break;
            case OxygenTier.Rebreather:
                PlayerData.OxygenMax = 1200f;
                _baseDrainRate = 0.5f;
                break;
        }
        PlayerData.OxygenCurrent = PlayerData.OxygenMax;
    }
        
    public System.Action<float> OnOxygenDepeleted;
    public System.Action<float> OnCritical;
    public System.Action<float> OnSafe;
    public System.Action<float> OnCaution;
}