using UnityEngine;

/// <summary>
/// Generates and plays procedural sound effects for movement, air blasts, suction, and impacts.
/// Eliminates missing audio asset dependencies by synthesizing custom waveforms in code.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayerAudioEffects : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioSource loopAudioSource;
    private AudioSource slideAudioSource;
    private AudioSource windAudioSource;

    private AudioClip footstepClip;
    private AudioClip windBlastClip;
    private AudioClip suctionHumClip;
    private AudioClip crateLaunchClip;
    private AudioClip slowMoEnterClip;
    private AudioClip slowMoExitClip;
    private AudioClip impactCrunchClip;
    private AudioClip slideStartClip;
    private AudioClip slideBoostClip;
    private AudioClip slideScrapeLoopClip;
    private AudioClip windRushLoopClip;
    private AudioClip grappleAttachClip;
    private AudioClip grappleFlingClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D player audio

        // Create secondary looping audio source for vacuum suction
        GameObject loopObj = new GameObject("LoopAudioSource");
        loopObj.transform.SetParent(transform);
        loopAudioSource = loopObj.AddComponent<AudioSource>();
        loopAudioSource.playOnAwake = false;
        loopAudioSource.loop = true;
        loopAudioSource.spatialBlend = 0f;

        // Slide Scrape Friction Audio Source
        GameObject slideObj = new GameObject("SlideAudioSource");
        slideObj.transform.SetParent(transform);
        slideAudioSource = slideObj.AddComponent<AudioSource>();
        slideAudioSource.playOnAwake = false;
        slideAudioSource.loop = true;
        slideAudioSource.spatialBlend = 0f;

        // Wind Rush Crescendo Audio Source
        GameObject windObj = new GameObject("WindRushAudioSource");
        windObj.transform.SetParent(transform);
        windAudioSource = windObj.AddComponent<AudioSource>();
        windAudioSource.playOnAwake = false;
        windAudioSource.loop = true;
        windAudioSource.spatialBlend = 0f;

        GenerateClips();
    }

    private void GenerateClips()
    {
        footstepClip = CreateFootstepClip();
        windBlastClip = CreateWindBlastClip();
        suctionHumClip = CreateSuctionHumClip();
        crateLaunchClip = CreateCrateLaunchClip();
        slowMoEnterClip = CreateSlowMoDropClip(true);
        slowMoExitClip = CreateSlowMoDropClip(false);
        impactCrunchClip = CreateImpactCrunchClip();
        slideStartClip = CreateSlideStartClip();
        slideBoostClip = CreateSlideBoostClip();
        slideScrapeLoopClip = CreateSlideScrapeLoopClip();
        windRushLoopClip = CreateWindRushLoopClip();
        grappleAttachClip = CreateGrappleAttachClip();
        grappleFlingClip = CreateGrappleFlingClip();

        loopAudioSource.clip = suctionHumClip;
        slideAudioSource.clip = slideScrapeLoopClip;
        windAudioSource.clip = windRushLoopClip;

        slideAudioSource.volume = 0f;
        slideAudioSource.Play();

        windAudioSource.volume = 0f;
        windAudioSource.Play();
    }

    public void PlayFootstep()
    {
        if (audioSource != null && footstepClip != null)
        {
            audioSource.pitch = Random.Range(0.92f, 1.08f);
            audioSource.PlayOneShot(footstepClip, 0.45f);
        }
    }

    public void PlayWindBlast()
    {
        if (audioSource != null && windBlastClip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(windBlastClip, 1.0f);
        }
    }

    public void SetSuctionLoop(bool active)
    {
        if (loopAudioSource == null) return;

        if (active && !loopAudioSource.isPlaying)
        {
            loopAudioSource.volume = 0.5f;
            loopAudioSource.Play();
        }
        else if (!active && loopAudioSource.isPlaying)
        {
            loopAudioSource.Stop();
        }
    }

    public void PlayCrateLaunch()
    {
        if (audioSource != null && crateLaunchClip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(crateLaunchClip, 1.0f);
        }
    }

    public void PlaySlowMoEnter()
    {
        if (audioSource != null && slowMoEnterClip != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(slowMoEnterClip, 0.8f);
        }
    }

    public void PlaySlowMoExit()
    {
        if (audioSource != null && slowMoExitClip != null)
        {
            audioSource.pitch = 1.1f;
            audioSource.PlayOneShot(slowMoExitClip, 0.7f);
        }
    }

    public void PlayImpactCrunch(float volume = 0.7f)
    {
        if (audioSource != null && impactCrunchClip != null)
        {
            audioSource.pitch = Random.Range(0.85f, 1.15f);
            audioSource.PlayOneShot(impactCrunchClip, Mathf.Clamp01(volume));
        }
    }

    public void PlaySlideStart()
    {
        if (audioSource != null && slideStartClip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(slideStartClip, 0.6f);
        }
    }

    public void PlaySlideBoost()
    {
        if (audioSource != null && slideBoostClip != null)
        {
            audioSource.pitch = Random.Range(1.1f, 1.25f);
            audioSource.PlayOneShot(slideBoostClip, 0.85f);
        }
    }

    public void PlayGrappleAttach()
    {
        if (audioSource != null && grappleAttachClip != null)
        {
            audioSource.pitch = Random.Range(0.96f, 1.05f);
            audioSource.PlayOneShot(grappleAttachClip, 0.75f);
        }
    }

    public void PlayGrappleFling()
    {
        if (audioSource != null && grappleFlingClip != null)
        {
            audioSource.pitch = Random.Range(1.05f, 1.2f);
            audioSource.PlayOneShot(grappleFlingClip, 0.9f);
        }
    }

    /// <summary>
    /// Dynamically modulates slide friction audio pitch and volume based on current speed.
    /// High pitch at 18+ m/s, dropping smoothly to a low rumble as friction slows the player down.
    /// </summary>
    public void UpdateSlideAudio(bool isSliding, float currentSpeed)
    {
        if (slideAudioSource == null) return;

        float dt = Time.deltaTime;
        if (isSliding && currentSpeed > 1.5f)
        {
            float speedRatio = Mathf.Clamp01((currentSpeed - 2.5f) / 18.0f);
            float targetVol = Mathf.Lerp(0.2f, 0.65f, speedRatio);
            float targetPitch = Mathf.Lerp(0.75f, 1.35f, speedRatio);

            slideAudioSource.volume = Mathf.MoveTowards(slideAudioSource.volume, targetVol, 5.0f * dt);
            slideAudioSource.pitch = Mathf.MoveTowards(slideAudioSource.pitch, targetPitch, 4.0f * dt);
        }
        else
        {
            slideAudioSource.volume = Mathf.MoveTowards(slideAudioSource.volume, 0f, 6.0f * dt);
        }
    }

    /// <summary>
    /// Ambient aerodynamic wind crescendo that builds smoothly as velocity exceeds 10 m/s up to 35 m/s.
    /// </summary>
    public void UpdateWindRush(float currentSpeed)
    {
        if (windAudioSource == null) return;

        float dt = Time.deltaTime;
        if (currentSpeed > 9.0f)
        {
            float ratio = Mathf.Clamp01((currentSpeed - 9.0f) / 24.0f);
            float targetVol = Mathf.Lerp(0.05f, 0.55f, ratio * ratio);
            float targetPitch = Mathf.Lerp(0.85f, 1.3f, ratio);

            windAudioSource.volume = Mathf.MoveTowards(windAudioSource.volume, targetVol, 3.0f * dt);
            windAudioSource.pitch = Mathf.MoveTowards(windAudioSource.pitch, targetPitch, 2.5f * dt);
        }
        else
        {
            windAudioSource.volume = Mathf.MoveTowards(windAudioSource.volume, 0f, 4.0f * dt);
        }
    }

    // --- Waveform Generators (Pure Math) ---

    private AudioClip CreateFootstepClip()
    {
        int sampleRate = 44100;
        float duration = 0.08f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - ((float)i / samples);
            // Low thud (70Hz sine) + noise tap
            float sine = Mathf.Sin(2f * Mathf.PI * 75f * t);
            float noise = (Random.value * 2f - 1f) * 0.3f;
            data[i] = (sine + noise) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("FootstepProcedural", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateWindBlastClip()
    {
        int sampleRate = 44100;
        float duration = 0.32f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Exp(-t * 6f);
            float whiteNoise = Random.value * 2f - 1f;
            float subBass = Mathf.Sin(2f * Mathf.PI * 90f * (1f - t) * ((float)i / sampleRate));
            data[i] = (whiteNoise * 0.7f + subBass * 0.6f) * envelope;
        }

        AudioClip clip = AudioClip.Create("WindBlastProcedural", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateSuctionHumClip()
    {
        int sampleRate = 44100;
        float duration = 0.5f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            // 110Hz turbine hum + harmonic 220Hz
            float hum = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.6f + Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.3f;
            float airFriction = (Random.value * 2f - 1f) * 0.15f;
            data[i] = (hum + airFriction) * 0.4f;
        }

        AudioClip clip = AudioClip.Create("SuctionHumProcedural", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateCrateLaunchClip()
    {
        int sampleRate = 44100;
        float duration = 0.35f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Exp(-t * 8f);
            float punch = Mathf.Sin(2f * Mathf.PI * 140f * (1f - t * 0.5f) * ((float)i / sampleRate));
            float hiss = (Random.value * 2f - 1f) * 0.5f;
            data[i] = (punch * 0.8f + hiss * 0.4f) * envelope;
        }

        AudioClip clip = AudioClip.Create("CrateLaunchProcedural", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateSlowMoDropClip(bool entering)
    {
        int sampleRate = 44100;
        float duration = 0.4f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Sin(t * Mathf.PI);
            float freq = entering ? Mathf.Lerp(160f, 55f, t) : Mathf.Lerp(55f, 160f, t);
            float sine = Mathf.Sin(2f * Mathf.PI * freq * ((float)i / sampleRate));
            data[i] = sine * envelope * 0.6f;
        }

        AudioClip clip = AudioClip.Create(entering ? "SlowMoEnter" : "SlowMoExit", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateImpactCrunchClip()
    {
        int sampleRate = 44100;
        float duration = 0.22f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Exp(-t * 12f);
            float lowClang = Mathf.Sin(2f * Mathf.PI * 120f * ((float)i / sampleRate));
            float noiseCrack = (Random.value * 2f - 1f) * 0.6f;
            data[i] = (lowClang * 0.5f + noiseCrack * 0.7f) * envelope;
        }

        AudioClip clip = AudioClip.Create("ImpactCrunchProcedural", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateSlideStartClip()
    {
        int sampleRate = 44100;
        float duration = 0.35f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Sin(t * Mathf.PI);
            float noise = (Random.value * 2f - 1f) * 0.45f;
            float frictionTone = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(300f, 150f, t) * ((float)i / sampleRate)) * 0.35f;
            data[i] = (noise + frictionTone) * envelope;
        }

        AudioClip clip = AudioClip.Create("SlideStartProcedural", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateSlideBoostClip()
    {
        int sampleRate = 44100;
        float duration = 0.3f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Sin(Mathf.Pow(1f - t, 0.5f) * Mathf.PI);
            float lowPunch = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(180f, 60f, t) * ((float)i / sampleRate)) * 0.6f;
            float rushNoise = (Random.value * 2f - 1f) * 0.4f * (1f - t);
            data[i] = (lowPunch + rushNoise) * envelope;
        }

        AudioClip clip = AudioClip.Create("SlideBoostProcedural", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateSlideScrapeLoopClip()
    {
        int sampleRate = 44100;
        float duration = 1.0f; // Seamless 1-second friction loop
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float noise = (Random.value * 2f - 1f) * 0.5f;
            float lowRumble = Mathf.Sin(2f * Mathf.PI * 110f * ((float)i / sampleRate)) * 0.35f;
            float gravelTone = Mathf.Sin(2f * Mathf.PI * 240f * ((float)i / sampleRate)) * 0.2f;
            data[i] = (noise + lowRumble + gravelTone) * 0.4f;
        }

        AudioClip clip = AudioClip.Create("SlideScrapeLoop", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateWindRushLoopClip()
    {
        int sampleRate = 44100;
        float duration = 1.2f; // Seamless 1.2-second aerodynamic wind loop
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float whiteNoise = (Random.value * 2f - 1f) * 0.35f;
            float lowWoosh = Mathf.Sin(2f * Mathf.PI * 80f * ((float)i / sampleRate)) * 0.45f;
            data[i] = (whiteNoise + lowWoosh) * 0.35f;
        }

        AudioClip clip = AudioClip.Create("WindRushLoop", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateGrappleAttachClip()
    {
        int sampleRate = 44100;
        float duration = 0.18f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Exp(-t * 16f);
            float electricZip = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(1200f, 400f, t) * ((float)i / sampleRate));
            float thunk = Mathf.Sin(2f * Mathf.PI * 90f * ((float)i / sampleRate)) * 0.7f;
            data[i] = (electricZip * 0.4f + thunk) * envelope;
        }

        AudioClip clip = AudioClip.Create("GrappleAttach", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateGrappleFlingClip()
    {
        int sampleRate = 44100;
        float duration = 0.28f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Sin(t * Mathf.PI);
            float whoosh = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(250f, 750f, t) * ((float)i / sampleRate));
            float snap = (Random.value * 2f - 1f) * (1f - t) * 0.5f;
            data[i] = (whoosh * 0.6f + snap) * envelope;
        }

        AudioClip clip = AudioClip.Create("GrappleFling", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
