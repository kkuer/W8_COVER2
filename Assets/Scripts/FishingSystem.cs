using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;

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
    public UnityEngine.UI.Slider catchingSlider;

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

    public bool castSuccess = false;
    public bool isBobbing = false;
    public bool isCatching = false;
    public bool catchSuccess = false;
    public bool bobCountdown = false;

    public bool canCast = true;

    Coroutine waitForFishCoroutine = null;
    Coroutine bobCountdownTimerCoroutine = null;

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
                catchingSlider.minValue = 0;
                catchingSlider.maxValue = mashAmount;
                catchingSlider.value = Mathf.RoundToInt(fishValue/2);
                catchingSliderObject.SetActive(true);
            }

            if (catchingSlider.value >= catchingSlider.maxValue)
            {
                isCatching = false;
                catchingPhase = false;
                catchingSliderObject.SetActive(false);

                mashAmount = 0;

                catchSuccess = true;
                afterRoundPhase = true;
            }
            else if (catchingSlider.value <= catchingSlider.minValue)
            {
                isCatching = false;
                catchingPhase = false;
                catchingSliderObject.SetActive(false);
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
                    newIndicator.gameObject.transform.Translate(Vector3.forward * Time.deltaTime);
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
                    catchingSlider.value++;
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
            catchingSlider.value -= 5f * Time.deltaTime;
        }
    }

    public IEnumerator waitForFish()
    {
        isBobbing = false;
        yield return new WaitForSeconds(Random.Range(3f, 7f));
        fishValue = Random.Range(10, 50);
        bobCountdown = true;
        isBobbing = true;
    }

    public IEnumerator showFish()
    {
        catchText.text = "You caught a $" + fishValue + " fish!";
        catchUI.SetActive(true);
        money += fishValue;
        yield return new WaitForSeconds(3f);
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
}