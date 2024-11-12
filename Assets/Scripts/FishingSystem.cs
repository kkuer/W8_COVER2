using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class FishingSystem : MonoBehaviour
{
    public Vector3 startCastPos = new Vector3(0, 0, -8);
    public Vector3 maxCastPos = new Vector3(0, 0, 3);

    public GameObject castPosIndicator;
    public GameObject bobber;

    public GameObject newBobber;
    public GameObject newIndicator;

    public GameObject introUI;

    public GameObject catchUI;
    public TMP_Text catchText;

    public TMP_Text moneyText;

    public ParticleSystem bobbingParticles;

    public GameObject catchingSliderObject;
    public GameObject catchingSliderObjectChild;
    public UnityEngine.UI.Slider catchingSlider;
    public UnityEngine.UI.Slider cooldownSlider;

    public GameObject mashTextObject;
    public GameObject mashAmountTextObject;
    public TMP_Text mashAmountText;
    public GameObject shatter;

    public GameObject cam1;
    public CinemachineVirtualCamera cineCam;

    public Color sliderCooldown;
    public Color slider2_2;

    public float mashAmount = 0f;

    public float fishValue = 0f;

    public float money = 0f;

    public bool roundActive = false;

    public bool neutralPhase = false;
    public bool castPhase = false;
    public bool waitingPhase = false;
    public bool bobbingPhase = false;
    public bool catchingPhase = false;
    public bool afterRoundPhase = false;

    public bool cooldownStarted = false;

    public bool castSuccess = false;
    public bool isBobbing = false;
    public bool isCatching = false;
    public bool catchSuccess = false;
    public bool bobCountdown = false;

    public bool catchCooldown = true;

    public bool canCast = true;
    public bool canMash = false;

    Coroutine waitForFishCoroutine = null;
    Coroutine bobCountdownTimerCoroutine = null;
    Coroutine catchCooldownCoroutine = null;

    public GameObject rodPos;
    public LineRenderer fishingLine;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (newBobber != null)
        {
            fishingLine.SetPosition(0, rodPos.transform.position);
            fishingLine.SetPosition(0, newBobber.transform.position);
        }
        else
        {
            fishingLine.SetPosition(0, rodPos.transform.position);
            fishingLine.SetPosition(1, rodPos.transform.position);
        }

        moneyText.text = "$" + money;

        if (!roundActive && !afterRoundPhase && canCast)
        {
            introUI.SetActive(true);

            if (newBobber != null)
            {
                bobbingParticles = null;
                Destroy(newBobber.gameObject);
                newBobber = null;
            }
            if (newIndicator != null)
            {
                Destroy(newIndicator.gameObject);
            }
        }

        if (waitingPhase)
        {
            if (castSuccess)
            {
                waitForFishCoroutine = StartCoroutine(waitForFish());
                castSuccess = false;
            }
        }

        if (isBobbing)
        {
            if (bobCountdown)
            {
                bobCountdownTimerCoroutine = StartCoroutine(bobCountdownTimer());
                bobCountdown = false;
            }
            bobbingParticles = newBobber.GetComponent<ParticleSystem>();
            if (!bobbingParticles.isPlaying)
            {
                bobbingParticles.Play();
            }
        }

        if (catchingPhase)
        {
            if (!isCatching)
            {
                isCatching = true;
                mashAmount = fishValue;
                catchingSlider.minValue = 10;
                catchingSlider.maxValue = mashAmount + 10;
                catchingSlider.value = Mathf.RoundToInt(((catchingSlider.maxValue - catchingSlider.minValue) / 3) + 10);
                mashAmountText.text = mashAmount.ToString();
                catchingSliderObjectChild.SetActive(false);
                mashTextObject.SetActive(false);
                catchingSliderObject.SetActive(true);
            }

            if (catchingSlider.value >= catchingSlider.maxValue)
            {
                isCatching = false;
                catchingPhase = false;
                canMash = false;
                catchingSliderObject.SetActive(false);

                mashAmount = 0;
                mashTextObject.SetActive(false);

                catchSuccess = true;
                afterRoundPhase = true;
                //move cinemachine cam
            }
            else if (catchingSlider.value <= 10 && !catchCooldown)
            {
                if (!catchCooldown)
                {
                    StopCoroutine(catchCooldownCoroutine);
                    isCatching = false;
                    catchingPhase = false;
                    canMash = false;
                    catchCooldown = true;
                    catchingSliderObject.SetActive(false);
                    shatter.SetActive(true);
                    roundActive = false;
                    canCast = false;
                    bobbingParticles = null;
                    Destroy(newBobber.gameObject);
                    newBobber = null;
                    canCast = true;
                }
            }

            if (((catchingSlider.value - catchingSlider.minValue) / (catchingSlider.maxValue - catchingSlider.minValue)) * (cooldownSlider.maxValue - cooldownSlider.minValue) + cooldownSlider.minValue <= cooldownSlider.value && !catchCooldown)
            {
                Debug.Log(((catchingSlider.value - catchingSlider.minValue) / (catchingSlider.maxValue - catchingSlider.minValue)) * (cooldownSlider.maxValue - cooldownSlider.minValue) + cooldownSlider.minValue);
                Debug.Log(cooldownSlider.value);
                Debug.Log("trigger2");
                StopCoroutine(catchCooldownCoroutine);
                isCatching = false;
                catchingPhase = false;
                canMash = false;
                catchCooldown = true;
                cooldownSlider.value = 0;
                catchingSliderObject.SetActive(false);
                shatter.SetActive(true);
                roundActive = false;
                canCast = false;
                bobbingParticles = null;
                Destroy(newBobber.gameObject);
                newBobber = null;
                canCast = true;
            }
        }

        if (afterRoundPhase)
        {
            roundActive = false;
            if (catchSuccess)
            {
                bobbingParticles = null;
                Destroy(newBobber.gameObject);
                newBobber = null;
                catchSuccess = false;

                catchCooldown = true;
                cooldownSlider.value = 0;
                shatter.gameObject.SetActive(true);
                cooldownSlider.value = 0;

                StartCoroutine(showFish());
            }
        }

        if (Input.GetKey(KeyCode.Space))
        {
            if (!roundActive && !afterRoundPhase && canCast)
            {
                roundActive = true;
                neutralPhase = true;
                introUI.SetActive(false);
            }

            if (neutralPhase)
            {
                //instantiate indicator
                if (!newIndicator)
                {
                    newIndicator = Instantiate(castPosIndicator, startCastPos, Quaternion.identity);
                }
                if (newIndicator.transform.position.z < maxCastPos.z)
                {
                    //move indicator
                    newIndicator.gameObject.transform.Translate(Vector3.forward * 1f * Time.deltaTime);
                }
            }
            else if (waitingPhase)
            {
                if (isBobbing)
                {
                    if (bobCountdownTimerCoroutine != null)
                    {
                        StopCoroutine(bobCountdownTimerCoroutine);
                        bobCountdownTimerCoroutine = null;
                    }
                    waitingPhase = false;
                    if (!cooldownStarted)
                    {
                        cooldownStarted = true;
                        catchCooldownCoroutine = StartCoroutine(catchingCooldown());
                    }
                    catchingPhase = true;
                    isCatching = false;
                    isBobbing = false;
                }
                else if (!isBobbing)
                {
                    StopCoroutine(waitForFishCoroutine);
                    waitForFishCoroutine = null;
                    waitingPhase = false;
                    roundActive = false;
                    canCast = false;
                    bobbingParticles = null;
                    Destroy(newBobber.gameObject);
                    newBobber = null;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (catchingPhase)
            {
                if (isCatching)
                {
                    if (canMash)
                    {
                        catchingSlider.value++;
                    }
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (neutralPhase)
            {
                neutralPhase = false;
                if (!newBobber)
                {
                    newBobber = Instantiate(bobber, newIndicator.gameObject.transform.position, Quaternion.Euler(180, 0, 0));
                    Destroy(newIndicator.gameObject);
                    newIndicator = null;
                }

                waitingPhase = true;
                castSuccess = true;

            }
            else if (!canCast)
            {
                canCast = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (catchingPhase)
        {
            if (!catchCooldown)
            {
                ShakeCamera(1f);
                if (fishValue <= 25)
                {
                    catchingSlider.value -= 2f * Time.deltaTime;
                }
                else if (fishValue >= 25)
                {
                    catchingSlider.value -= 5f * Time.deltaTime;
                }
                cooldownSlider.value += 0.1f * Time.deltaTime;
            }

            if (cooldownSlider.value < 0.28 && catchCooldown)
            {
                cooldownSlider.value += 0.1f * Time.deltaTime;
            }
        }
    }

    public IEnumerator waitForFish()
    {
        isBobbing = false;
        yield return new WaitForSeconds(Random.Range(3f, 7f));
        fishValue = Random.Range(15, 25);
        bobCountdown = true;
        isBobbing = true;
    }

    public IEnumerator showFish()
    {
        cam1.SetActive(false);
        ShakeCamera(0f);
        catchText.text = "You caught a $" + fishValue + " fish!";
        catchUI.SetActive(true);
        money += fishValue;
        yield return new WaitForSeconds(3f);
        cam1.SetActive(true);
        catchUI.SetActive(false);
        fishValue = 0f;
        afterRoundPhase = false;
        canCast = true;
    }

    public IEnumerator bobCountdownTimer()
    {
        yield return new WaitForSeconds(2f);
        StopCoroutine(waitForFishCoroutine);
        waitForFishCoroutine = null;
        waitingPhase = false;
        roundActive = false;
        canCast = false;
        isBobbing = false;
        bobbingParticles = null;
        Destroy(newBobber.gameObject);
        newBobber = null;
        canCast = true;
    }

    public IEnumerator catchingCooldown()
    {
        catchCooldown = true;
        mashAmountTextObject.SetActive(true);
        yield return new WaitForSeconds(3f);

        cooldownSlider.value = 0f;
        catchCooldown = false;
        canMash = true;

        shatter.gameObject.SetActive(false);
        catchingSliderObjectChild.SetActive(true);
        mashAmountTextObject.SetActive(false);
        mashAmountText.text = "0";
        mashTextObject.SetActive(true);
        cooldownStarted = false;
    }

    public void ShakeCamera(float intensity)
    {
        CinemachineBasicMultiChannelPerlin cineShaker = cineCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cineShaker.m_AmplitudeGain = intensity;
    }
}