using UnityEngine;

[RequireComponent(typeof(Light))]
public class WwiseReactiveLight : MonoBehaviour
{
    [Header("Wwise Meter RTPC")]

    // name of the rtpc from wwise
    [Tooltip("Exact name of the Game Parameter receiving the Wwise Meter output.")]
    [SerializeField]
    private string meterRtpcName = "Music_Level";

    // only need this if the rtpc is on a gameobject
    [Tooltip(
        "Leave empty when the RTPC is global. " +
        "Assign the Wwise emitter only for a GameObject-scoped RTPC."
    )]
    [SerializeField]
    private GameObject wwiseGameObject;

    [Header("Light")]

    // light i want to control
    [SerializeField]
    private Light targetLight;

    // lowest light value
    [SerializeField, Min(0f)]
    private float minimumIntensity = 0f;

    // highest light value
    [SerializeField, Min(0f)]
    private float maximumIntensity = 4f;

    [Header("Wwise Meter Range")]

    // quietest db value
    [Tooltip("Meter value that maps to normalized loudness 0.")]
    [SerializeField]
    private float minimumDecibels = -48f;

    // loudest db value
    [Tooltip("Meter value that maps to normalized loudness 1.")]
    [SerializeField]
    private float maximumDecibels = 0f;

    [Header("Loudness Threshold")]

    // dont react until it gets loud enough
    [Tooltip(
        "The light stays at minimum intensity below this normalized loudness. " +
        "For a -48 to 0 dB range, 0.75 starts reacting at approximately -12 dB."
    )]
    [SerializeField, Range(0f, 0.99f)]
    private float activationThreshold = 0.75f;

    // makes louder sounds stand out more
    [Tooltip(
        "Higher values make the light respond mostly to the loudest moments."
    )]
    [SerializeField, Min(0.1f)]
    private float intensityPower = 3f;

    [Header("Breathing Response")]

    // how fast it gets brighter
    [Tooltip("How long the light takes to become brighter.")]
    [SerializeField, Min(0.001f)]
    private float attackTime = 0.6f;

    // how fast it gets darker
    [Tooltip("How long the light takes to fade.")]
    [SerializeField, Min(0.001f)]
    private float releaseTime = 1.8f;

    [Header("Debug — read only during Play Mode")]

    // current db value from wwise
    [SerializeField]
    private float currentDecibels;

    // db changed into 0 to 1
    [SerializeField, Range(0f, 1f)]
    private float normalizedLoudness;

    // value after threshold
    [SerializeField, Range(0f, 1f)]
    private float thresholdedLoudness;

    // light value i want to reach
    [SerializeField]
    private float targetIntensity;

    // stores if wwise worked or not
    [SerializeField]
    private AKRESULT lastWwiseResult;

    // needed for smoothdamp
    private float intensityVelocity;

    // id version of the rtpc name
    private uint meterRtpcId;

    // invalid ids if nothing is used
    private const ulong InvalidGameObjectId = ulong.MaxValue;
    private const uint InvalidPlayingId = 0;

    private void Awake()
    {
        // get the light if i didnt drag one in
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }

        // stop if there is no light
        if (targetLight == null)
        {
            Debug.LogError(
                "WwiseReactiveLight requires a Light component.",
                this
            );

            enabled = false;
            return;
        }

        // stop if rtpc name is empty
        if (string.IsNullOrWhiteSpace(meterRtpcName))
        {
            Debug.LogError(
                "The Wwise meter RTPC name is empty.",
                this
            );

            enabled = false;
            return;
        }

        // turn the rtpc name into an id
        meterRtpcId =
            AkUnitySoundEngine.GetIDFromString(meterRtpcName);
    }

    private void Update()
    {
        // get current loudness
        currentDecibels = GetWwiseMeterValue();

        // change db into 0 to 1
        normalizedLoudness = Mathf.InverseLerp(
            minimumDecibels,
            maximumDecibels,
            currentDecibels
        );

        // keep it at 0 until it passes the threshold
        thresholdedLoudness = Mathf.InverseLerp(
            activationThreshold,
            1f,
            normalizedLoudness
        );

        // make loud parts stand out more
        thresholdedLoudness = Mathf.Pow(
            thresholdedLoudness,
            intensityPower
        );

        // change loudness into light intensity
        targetIntensity = Mathf.Lerp(
            minimumIntensity,
            maximumIntensity,
            thresholdedLoudness
        );

        // use different speed for going up and down
        float smoothingTime =
            targetIntensity > targetLight.intensity
                ? attackTime
                : releaseTime;

        // smoothly move to the new light value
        targetLight.intensity = Mathf.SmoothDamp(
            targetLight.intensity,
            targetIntensity,
            ref intensityVelocity,
            smoothingTime
        );
    }

    private float GetWwiseMeterValue()
    {
        // start with the quietest value
        float returnedValue = minimumDecibels;

        // check if using a gameobject rtpc
        bool useGameObjectScope = wwiseGameObject != null;

        // get the gameobject id if needed
        ulong gameObjectId = useGameObjectScope
            ? AkUnitySoundEngine.GetAkGameObjectID(
                wwiseGameObject
            )
            : InvalidGameObjectId;

        // tell wwise if its global or gameobject
        int requestedValueType = useGameObjectScope
            ? (int)AkQueryRTPCValue.RTPCValue_GameObject
            : (int)AkQueryRTPCValue.RTPCValue_Global;

        // ask wwise for the rtpc value
        lastWwiseResult = AkUnitySoundEngine.GetRTPCValue(
            meterRtpcId,
            gameObjectId,
            InvalidPlayingId,
            out returnedValue,
            ref requestedValueType
        );

        // if it failed just use the minimum
        if (lastWwiseResult != AKRESULT.AK_Success)
        {
            return minimumDecibels;
        }

        // give back the value
        return returnedValue;
    }

    private void OnValidate()
    {
        // make sure max is never lower than min
        maximumIntensity = Mathf.Max(
            maximumIntensity,
            minimumIntensity
        );

        // stop db range from breaking
        if (maximumDecibels <= minimumDecibels)
        {
            maximumDecibels = minimumDecibels + 1f;
        }

        // keep threshold in range
        activationThreshold = Mathf.Clamp(
            activationThreshold,
            0f,
            0.99f
        );

        // stop power going too low
        intensityPower = Mathf.Max(
            0.1f,
            intensityPower
        );

        // stop attack time being 0
        attackTime = Mathf.Max(
            0.001f,
            attackTime
        );

        // stop release time being 0
        releaseTime = Mathf.Max(
            0.001f,
            releaseTime
        );
    }
}