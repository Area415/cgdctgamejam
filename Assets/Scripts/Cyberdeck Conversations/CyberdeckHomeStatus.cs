using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the cyberdeck HOME tab: location, weather, and the job card.
///
/// Put this on the "HomeDashboard" object. It finds its pieces BY NAME
/// (Txt_LocationName, Txt_Pickup, Img_JobProgress, ...), so nothing needs to be
/// dragged in. Anything that doesn't exist is simply skipped.
///
/// Values are stored statically, so they survive the cyberdeck closing and
/// carry across scene loads. They change when:
///   - the camera moves to a Location whose District Name is filled in
///     (automatic: every camera command in Yarn), or
///   - Yarn runs one of the job commands registered in VisualNovel.cs:
///       set_job "CURRENT JOB" "IN PROGRESS" "#4153"
///       set_job_route "Boo Boo's Boba Emporium · Understory" "Penthouse · Meridian District"
///       set_job_payout "₿ 12.50 (−20% late fee)"
///       set_job_progress 0.35
/// </summary>
public class CyberdeckHomeStatus : MonoBehaviour
{
    // ---------- Current values. Defaults = the start of the game (Karen's job is done). ----------
    public static string LocationName, LocationSub, AirValue, AirNote;
    public static string JobHeader, JobStatus, JobId, Pickup, Dropoff, Payout;
    public static float  JobProgress;
    public static float  Rating;      // 0..5 stars
    public static string RatingNote;

    /// <summary>Fired whenever any value changes.</summary>
    public static event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void SetDefaults()
    {
        Changed = null; // clear listeners from a previous Play session

        LocationName = "UNDERSTORY";
        LocationSub  = "Frisscity · Lower Districts";
        AirValue     = "SMOG";
        AirNote      = "3";  // just the number: "AGENTS sats overhead:" is part of the card art

        JobHeader   = "LAST JOB";
        JobStatus   = "COMPLETED";
        JobId       = "#4152";
        Pickup      = "Fresh Produce – Understory";
        Dropoff     = "Karen F. – Understory";
        Payout      = "₿ 8.75 (tip: ₿ 0.00)";
        JobProgress = 1f;

        Rating     = 1f;              // Karen's review
        RatingNote = "1 new review";
    }

    // ---------- Called by game code / Yarn ----------

    // ---------- District info per camera location ----------
    // Keyed by the Location's object name (the same name used in Yarn's camera command).
    // Columns: district name, subtitle, air quality, AGENTS sats overhead.
    // Locations NOT in this list (BlackImage, CGs, endings...) keep the previous district.
    // You can also fill the fields on a Location in the Inspector; those win over this list.
    private static readonly System.Collections.Generic.Dictionary<string, string[]> Districts =
        new System.Collections.Generic.Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        // Understory: the smoggy lower districts
        { "Alleyway",        new[] { "UNDERSTORY",        "Frisscity · Lower Districts", "SMOG",     "3"  } },
        { "AlleywayClear",   new[] { "UNDERSTORY",        "Frisscity · Lower Districts", "SMOG",     "3"  } },
        { "Understory",      new[] { "UNDERSTORY",        "Frisscity · Lower Districts", "SMOG",     "3"  } },

        // Meridian: rich towers, filtered air, AGENTS everywhere
        { "Lobby",           new[] { "MERIDIAN DISTRICT", "Frisscity · Upper Districts", "FILTERED", "12" } },
        { "PenthouseClosed", new[] { "MERIDIAN DISTRICT", "Frisscity · Upper Districts", "FILTERED", "12" } },
        { "PenthouseOpen",   new[] { "MERIDIAN DISTRICT", "Frisscity · Upper Districts", "FILTERED", "12" } },

        // The Bridge: out over the water, the air clears
        { "Bridge",          new[] { "THE BRIDGE",        "Frisscity → Root Town",       "CLEAR",    "1"  } },
        { "BridgeBattle",    new[] { "THE BRIDGE",        "Frisscity → Root Town",       "CLEAR",    "1"  } },

        // Root Town: off the AGENTS grid
        { "BridgeRootTown",  new[] { "ROOT TOWN",         "Area 415 · Off-grid",         "FRESH",    "0"  } },
        { "GirlsApt",        new[] { "ROOT TOWN",         "Area 415 · Off-grid",         "FRESH",    "0"  } },
        { "OutsideGirlsApt", new[] { "ROOT TOWN",         "Area 415 · Off-grid",         "FRESH",    "0"  } },
    };

    /// <summary>Called automatically when the camera moves to a Location.</summary>
    public static void SetLocation(Location location)
    {
        if (location == null) return;

        // 1) Fields filled in on the Location (Inspector) win.
        if (!string.IsNullOrWhiteSpace(location.districtName))
        {
            LocationName = location.districtName;
            if (!string.IsNullOrWhiteSpace(location.districtSubtitle)) LocationSub = location.districtSubtitle;
            if (!string.IsNullOrWhiteSpace(location.airValue))         AirValue    = location.airValue;
            if (!string.IsNullOrWhiteSpace(location.airNote))          AirNote     = location.airNote;
            Changed?.Invoke();
            return;
        }

        // 2) Otherwise use the list above. Not listed = keep the previous district.
        if (Districts.TryGetValue(location.name, out var d))
        {
            LocationName = d[0];
            LocationSub  = d[1];
            AirValue     = d[2];
            AirNote      = d[3];
            Changed?.Invoke();
        }
    }

    /// <summary>set_job "CURRENT JOB" "IN PROGRESS" "#4153"</summary>
    public static void SetJob(string header, string status, string jobId)
    {
        JobHeader = header;
        JobStatus = status;
        JobId     = jobId;
        Changed?.Invoke();
    }

    /// <summary>set_job_route "pickup place" "drop-off place"</summary>
    public static void SetJobRoute(string pickup, string dropoff)
    {
        Pickup  = pickup;
        Dropoff = dropoff;
        Changed?.Invoke();
    }

    /// <summary>set_job_payout "₿ 12.50 (−20% late fee)"</summary>
    public static void SetJobPayout(string payout)
    {
        Payout = payout;
        Changed?.Invoke();
    }

    /// <summary>set_job_progress 0.35   (0 = just picked up, 1 = delivered)</summary>
    public static void SetJobProgress(float progress)
    {
        JobProgress = Mathf.Clamp01(progress);
        Changed?.Invoke();
    }

    /// <summary>set_rating 5 "1 new review"   (stars 0-5; halves like 2.5 work too)</summary>
    public static void SetRating(float stars, string note)
    {
        Rating     = Mathf.Clamp(stars, 0f, 5f);
        RatingNote = note;
        Changed?.Invoke();
    }

    // ---------- Updating what's on screen ----------

    private TMP_Text txtLocationName, txtLocationSub, txtAirValue, txtAirNote,
                     txtJobHeader, txtJobStatus, txtJobId, txtPickup, txtDropoff, txtPayout,
                     txtRatingNote;
    private Image imgJobProgress, imgRatingStars;
    // Optional pill images: whichever matches JobStatus is shown ("IN PROGRESS" -> InProgress, "COMPLETED" -> Completed).
    private GameObject pillInProgress, pillCompleted;

    private void Awake()
    {
        txtLocationName = FindText("Txt_LocationName");
        txtLocationSub  = FindText("Txt_LocationSub");
        txtAirValue     = FindText("Txt_AirValue");
        txtAirNote      = FindText("Txt_AirNote");
        txtJobHeader    = FindText("Txt_JobHeader");
        txtJobStatus    = FindText("Txt_JobStatus");
        txtJobId        = FindText("Txt_JobId");
        txtPickup       = FindText("Txt_Pickup");
        txtDropoff      = FindText("Txt_Dropoff");
        txtPayout       = FindText("Txt_Payout");
        imgJobProgress  = FindImage("Img_JobProgress");
        txtRatingNote   = FindText("Txt_RatingNote");
        imgRatingStars  = FindImage("Img_RatingStars");
        pillInProgress  = FindObject("InProgress");
        pillCompleted   = FindObject("Completed");

        if (imgJobProgress != null && imgJobProgress.type != Image.Type.Filled)
        {
            Debug.LogWarning("CyberdeckHomeStatus: set Img_JobProgress's Image Type to 'Filled' (Horizontal) so the bar can fill.", imgJobProgress);
        }
    }

    private void OnEnable()
    {
        Changed += Refresh;
        Refresh();
    }

    private void OnDisable() => Changed -= Refresh;

    private void Refresh()
    {
        Set(txtLocationName, LocationName);
        Set(txtLocationSub,  LocationSub);
        Set(txtAirValue,     AirValue);
        Set(txtAirNote,      AirNote);
        Set(txtJobHeader,    JobHeader);
        Set(txtJobStatus,    JobStatus);
        Set(txtJobId,        JobId);
        Set(txtPickup,       Pickup);
        Set(txtDropoff,      Dropoff);
        Set(txtPayout,       Payout);
        if (imgJobProgress != null) imgJobProgress.fillAmount = JobProgress;
        Set(txtRatingNote, RatingNote);
        if (imgRatingStars != null) imgRatingStars.fillAmount = Rating / 5f;

        bool completed = JobStatus != null && JobStatus.Trim().ToUpperInvariant() == "COMPLETED";
        if (pillCompleted  != null) pillCompleted.SetActive(completed);
        if (pillInProgress != null) pillInProgress.SetActive(!completed);
    }

    private static void Set(TMP_Text t, string value)
    {
        if (t != null) t.text = value;
    }

    private TMP_Text FindText(string objectName)
    {
        foreach (var t in GetComponentsInChildren<TMP_Text>(true))
            if (t.gameObject.name == objectName) return t;
        return null;
    }

    private GameObject FindObject(string objectName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.gameObject.name == objectName) return t.gameObject;
        return null;
    }

    private Image FindImage(string objectName)
    {
        foreach (var i in GetComponentsInChildren<Image>(true))
            if (i.gameObject.name == objectName) return i;
        return null;
    }
}
