using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    public GameObject _playerObj;
    [Header("AudioEvents")]
    public AK.Wwise.Event Walk = new AK.Wwise.Event();
    public AK.Wwise.Event WallClimb = new AK.Wwise.Event();
    public AK.Wwise.Event IdleVoice = new AK.Wwise.Event();
    [Header("AK - Envelope Follower")]
    public AK.Wwise.RTPC talkingVol = new AK.Wwise.RTPC();
    [Header("CallbackFlags")]
    public AK.Wwise.CallbackFlags CallbackFlags;

    private Animator anim;
    private Movement move;
    private Collision coll;
    [HideInInspector]
    public SpriteRenderer sr;
    public GameObject go;
    private Rigidbody2D rb;

    [Header("Attach the Player GO")]
    public Movement movement;

    private bool canTalk;
    private bool canReallyTalk;
    public float spriteSizeMin = 1f;
    public float spriteSizeMax = 1.2f;
    private float rtpcValueTalkingVol;//store the volume envelope.

    public string[] saySomething;

    public bool CanTalkOrNot
    {
        get { return canTalk; }
        set
        {
            if (value == canTalk)
                return;

            canTalk = value;
            if (canTalk)
            {
                StopAllCoroutines();
                StartCoroutine(waitForRandomSeconds());
            }
            else
            {
                StopCoroutine(waitForRandomSeconds());
                canReallyTalk = false;
                Debug.LogWarning("can Really talk is false!");
            }
            IEnumerator waitForRandomSeconds()
            {
                yield return new WaitForSeconds(Random.Range(5f,7f));
                canReallyTalk = true;
                Debug.LogWarning("can Really talk is true!");
            }
        }
    }

    void Start()
    {
        CanTalkOrNot = true;//Character can talk at the beginning.
        anim = GetComponent<Animator>();
        coll = GetComponentInParent<Collision>();
        move = GetComponentInParent<Movement>();
        sr = GetComponent<SpriteRenderer>();
        rb = go.GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        anim.SetBool("onGround", coll.onGround);
        anim.SetBool("onWall", coll.onWall);
        anim.SetBool("onRightWall", coll.onRightWall);
        anim.SetBool("wallGrab", move.wallGrab);
        anim.SetBool("wallSlide", move.wallSlide);
        anim.SetBool("canMove", move.canMove);
        anim.SetBool("isDashing", move.isDashing);
        //if joystick hasn't been touched OR character is not moving, then when character is idle, will talk.
        if ((Mathf.Abs(movement.horizontalAxis) == 0 && Mathf.Abs(movement.verticalAxis) == 0) || 
            (Mathf.Abs(movement.rb.velocity.x) == 0 && Mathf.Abs(movement.rb.velocity.y) == 0))
        {
            CanTalkOrNot = true;
        }
        else
        {
            CanTalkOrNot = false;
        }

        //Debug.LogWarning(canTalk);
        int valueTypeTalkingVol = (int)AkQueryRTPCValue.RTPCValue_GameObject;
        AKRESULT result = AkSoundEngine.GetRTPCValue(talkingVol.Name, go, 0, out rtpcValueTalkingVol, ref valueTypeTalkingVol);
        sr.transform.localScale = new Vector2(map(rtpcValueTalkingVol, -48f, 0f, spriteSizeMin, spriteSizeMax), 
            map(rtpcValueTalkingVol, -48f, 0f, spriteSizeMin, spriteSizeMax));
        sr.transform.localPosition = new Vector2(0, (map(rtpcValueTalkingVol, -48f, 0f, spriteSizeMin, spriteSizeMax)-1f)*0.5f);
    }
    public void SetHorizontalMovement(float x,float y, float yVel)
    {
        anim.SetFloat("HorizontalAxis", x);
        anim.SetFloat("VerticalAxis", y);
        anim.SetFloat("VerticalVelocity", yVel);
    }
    public void SetTrigger(string trigger)
    {
        anim.SetTrigger(trigger);
    }
    public void Flip(int side)
    {

        if (move.wallGrab || move.wallSlide)
        {
            if (side == -1 && sr.flipX)
                return;

            if (side == 1 && !sr.flipX)
            {
                return;
            }
        }

        bool state = (side == 1) ? false : true;
        sr.flipX = state;
    }

    public void Walk_AudioEvent()
    {
        AkSoundEngine.PostEvent(Walk.Id, _playerObj);
    }
	
    public void WallClimb_AudioEvent()
    {
        AkSoundEngine.PostEvent(WallClimb.Id, _playerObj);
    }

    public void IdleVoice_AudioEvent()
    {
        if (canReallyTalk)
        {
            AkSoundEngine.PostEvent(IdleVoice.Id, go, CallbackFlags.value, ec, null);
        }
    }

    public void ec(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
    {
        if (in_type == AkCallbackType.AK_Marker)
        {
            if (in_info != null)
            {
                Debug.Log(saySomething[Random.Range(0, saySomething.Length)]);
            }
        }
    }

    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}