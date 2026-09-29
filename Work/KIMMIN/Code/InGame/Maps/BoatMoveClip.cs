using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Work.Code.Map
{
    public class BoatMoveClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField] private AnimationCurve progressCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.ClipIn | ClipCaps.SpeedMultiplier;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<BoatMoveBehaviour> playable = ScriptPlayable<BoatMoveBehaviour>.Create(graph);
            playable.GetBehaviour().ProgressCurve = progressCurve;
            return playable;
        }
    }

    public class BoatMoveBehaviour : PlayableBehaviour
    {
        public AnimationCurve ProgressCurve { get; set; }

        public float Evaluate(float time)
        {
            return ProgressCurve.Evaluate(Mathf.Clamp01(time));
        }
    }
}
