using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using AK;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;

[UnityEngine.RequireComponent(typeof(AkGameObj))]

public class Movement : MonoBehaviour
{
    [Header("AkEvent")]
    public AK.Wwise.Event Jump_Event = new AK.Wwise.Event();
    public AK.Wwise.Event Landing_Event = new AK.Wwise.Event();
    public AK.Wwise.Event NaturalWallSlide_Start = new AK.Wwise.Event();
    public AK.Wwise.Event NaturalWallSlide_Stop = new AK.Wwise.Event();
    public AK.Wwise.Event GrabWallSlide_Start = new AK.Wwise.Event();
    public AK.Wwise.Event GrabWallSlide_Stop = new AK.Wwise.Event();
    public AK.Wwise.Event Falling_Event = new AK.Wwise.Event();
    public AK.Wwise.Event dash = new AK.Wwise.Event();

    [Header("AkRTPC")]
    public AK.Wwise.RTPC VelocityX = new AK.Wwise.RTPC();
    public AK.Wwise.RTPC VelocityY = new AK.Wwise.RTPC();
    public AK.Wwise.RTPC TimeFreezeGP = new AK.Wwise.RTPC();
    public AK.Wwise.RTPC Envelope = new AK.Wwise.RTPC();

    [Header("AKState")]
    public AK.Wwise.State FreezeOn = new AK.Wwise.State();
    public AK.Wwise.State FreezeOff = new AK.Wwise.State();

    [HideInInspector]
    private Collision coll;
    [HideInInspector]
    public Rigidbody2D rb;

    [Space]
    [Header("AnimationScript")]
    public AnimationScript anim;

    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed = 20;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallGrab;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;
    [Space]
    private bool groundTouch;
    private bool hasDashed;
    public int side = 1;

    private bool isFreezing;

    public float MaxTimeFrzDur = 15f;

    [Space]
    [Header("Polish")]
    public ParticleSystem dashParticle;
    public ParticleSystem jumpParticle;
    public ParticleSystem wallJumpParticle;
    public ParticleSystem slideParticle;

    private bool naturalWallSlideBool = false;
    private bool grabWallSlideBool = false;
    private bool fallingOrNot = false;
    public float fallingDelay = 0.5f;
    private float walking = 0f;

    public float horizontalAxis;
    public float verticalAxis;

    public float rtpcValueEnvelope;

    public CinemachineImpulseSource impulseSource;

    public ParticleSystem snowPS;
    public PostProcessVolume postProcessVolume;
    public LensDistortion ld;

    public bool NaturalWallSlideBool
    {
        get { return naturalWallSlideBool; }
        set
        {
            if (value == naturalWallSlideBool)
                return;

            naturalWallSlideBool = value;
            if (naturalWallSlideBool)
            {
                NaturalWallSlide_Start.Post(gameObject);
            }
            else
            {
                NaturalWallSlide_Stop.Post(gameObject);
            }
        }
    }
    public bool GrabWallSlideBool
    {
        get { return grabWallSlideBool; }
        set
        {
            if (value == grabWallSlideBool)
                return;

            grabWallSlideBool = value;
            if (grabWallSlideBool)
            {
                GrabWallSlide_Start.Post(gameObject);
            }
            else
            {
                GrabWallSlide_Stop.Post(gameObject);
            }
        }
    }
    public float WalkingOrNot
    {
        get { return walking; }
        set
        {
            if (value == 0)
                return;
            walking = value;
            if (walking > 0)
            {
                VelocityX.SetValue(gameObject, value*speed);
            }
        }
    }
    public bool FallingOrNot
    {
        get { return fallingOrNot; }
        set
        {
            if (value == fallingOrNot)
                return;
            fallingOrNot = value;
            if (fallingOrNot)
            {
                StopCoroutine(FallingDelay());
                StartCoroutine(FallingDelay());
            }
            if (!fallingOrNot)
            {
                Falling_Event.Stop(gameObject);
                StopCoroutine(FallingDelay());
                Debug.LogWarning("Not Falling!");
            }
            IEnumerator FallingDelay()
            {
                yield return new WaitForSeconds(fallingDelay);
                Debug.LogWarning("Falling!");
                Falling_Event.Post(gameObject);
            }
        }
    }
    public bool IsFreezing
    {
        get { return isFreezing; }
        set
        {
            if (value == isFreezing)
                return;
            isFreezing = value;
            if (isFreezing)
            {
                TimeFreeze();
                FreezeOn.SetValue();
                TimeFreezeGP.SetGlobalValue(1);
            }
            if (!isFreezing)
            {
                FreezeOff.SetValue();
                TimeFreezeGP.SetGlobalValue(0);
            }
        }
    }
    void Start()
    {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationScript>();
    }

    void Update()
    {

        float x = Input.GetAxis("Horizontal");
        horizontalAxis = x;
        float y = Input.GetAxis("Vertical");
        verticalAxis = y;
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);
        Walk(dir);
        anim.SetHorizontalMovement(x, y, rb.velocity.y);

        if (rb.velocity.y >= -9)
        {
            FallingOrNot = false;
        }
        else
        {
            FallingOrNot = true;
        }

        if (coll.onWall && Input.GetButton("WallGrab") && canMove)
        {
            if(side != coll.wallSide)
                anim.Flip(side*-1);
            wallGrab = true;
            wallSlide = false;
            NaturalWallSlideBool = false;
        }

        if (Input.GetButtonUp("WallGrab") || !coll.onWall || !canMove)
        {
            wallGrab = false;
            wallSlide = false;
            NaturalWallSlideBool = false;
        }

        if (coll.onGround && !isDashing)
        {
            wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;
        }
        
        if (wallGrab && !isDashing)
        {
            rb.gravityScale = 0;
            if(x > .2f || x < -.2f)
            rb.velocity = new Vector2(rb.velocity.x, 0);
            float speedModifier = y > 0 ? .5f : 1;
            rb.velocity = new Vector2(rb.velocity.x, y * (speed * speedModifier));
        }
        else
        {
            rb.gravityScale = 3;
        }

        if(coll.onWall && !coll.onGround)
        {
            if (x != 0 && !wallGrab)
            {
                wallSlide = true;
                NaturalWallSlideBool = true;
                WallSlide();
            }
        }

        if (!coll.onWall || coll.onGround)
        {
            wallSlide = false;
            NaturalWallSlideBool = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            anim.SetTrigger("jump");
            if (coll.onGround)
                Jump(Vector2.up, false);
            if (coll.onWall && !coll.onGround)
                WallJump();
        }

        if (Input.GetButtonDown("TimeFreeze") && !isFreezing)
        {
            IsFreezing = true;
        }

        if (Input.GetButtonDown("Dash") && !hasDashed)
        {
            if(xRaw != 0 || yRaw != 0)
                Dash(xRaw, yRaw);
        }

        if (coll.onGround && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if(!coll.onGround && groundTouch)
        {
            groundTouch = false;
        }

        WallParticle(y);

        if (wallGrab || wallSlide || !canMove)
            return;

        if(x > 0)
        {
            side = 1;
            anim.Flip(side);
        }

        if (x < 0)
        {
            side = -1;
            anim.Flip(side);
        }

        WalkingOrNot = Mathf.Abs(dir.x);
    }

    private void TimeFreeze()
    {
        var main = snowPS.main;
        main.simulationSpeed = 0;
        dash.Post(gameObject);
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));
        impulseSource.GenerateImpulse();
        StartCoroutine(FreezeWait());
    }
    IEnumerator FreezeWait()
    {
        IsFreezing = true;
        yield return new WaitForSeconds(MaxTimeFrzDur);
        IsFreezing = false;
        var main = snowPS.main;
        main.simulationSpeed = 1;
    }

    void GroundTouch()
    {
        Landing_Event.Post(gameObject);
        hasDashed = false;
        isDashing = false;
        side = anim.sr.flipX ? -1 : 1;
        jumpParticle.Play();
        VelocityY.SetValue(gameObject, - 1);
    }

    private void Dash(float x, float y)
    {
        dash.Post(gameObject);
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        //https://github.com/keijiro/RippleEffect

        FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));
        impulseSource.GenerateImpulse();
        hasDashed = true;
        
        anim.SetTrigger("dash");

        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);

        rb.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    IEnumerator DashWait()
    {
        FindObjectOfType<GhostTrail>().ShowGhost();
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        dashParticle.Play();
        rb.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        dashParticle.Stop();
        rb.gravityScale = 3;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (coll.onGround)
            hasDashed = false;
    }

    private void WallJump()
    {
        Jump_Event.Post(gameObject);
        if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        {
            side *= -1;
            anim.Flip(side);
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

        Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);

        wallJumped = true;
    }

    private void WallSlide()
    {
        if(coll.wallSide != side)
         anim.Flip(side * -1);

        if (!canMove)
            return;

        bool pushingWall = false;
        if((rb.velocity.x > 0 && coll.onRightWall) || (rb.velocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;

        rb.velocity = new Vector2(push, -slideSpeed);
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        if (wallGrab)
            return;

        if (!wallJumped)
        {

            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }


    private void Jump(Vector2 dir, bool wall)
    {
        Jump_Event.Post(gameObject);
        slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * jumpForce;

        particle.Play();
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    void RigidbodyDrag(float x)
    {
        rb.drag = x;
    }

    void WallParticle(float vertical)
    {
        var main = slideParticle.main;
        
        if (wallSlide || (wallGrab && vertical < 0))
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            main.startColor = Color.white;
            GrabWallSlideBool = true;
            VelocityY.SetValue(gameObject, rb.velocity.y);
        }
        else
        {
            main.startColor = Color.clear;
            GrabWallSlideBool = false;
        }
    }

    int ParticleSide()
    {
        int particleSide = coll.onRightWall ? 1 : -1;
        return particleSide;
    }
}
