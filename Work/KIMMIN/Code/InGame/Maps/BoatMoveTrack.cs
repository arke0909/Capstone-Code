using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Work.Code.Map
{
    [TrackClipType(typeof(BoatMoveClip))]
    [TrackBindingType(typeof(BoatTimelineMover))]
    public class BoatMoveTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<BoatMoveMixer>.Create(graph, inputCount);
        }
    }

    public class BoatMoveMixer : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            BoatTimelineMover mover = (BoatTimelineMover)playerData;
            int inputCount = playable.GetInputCount();
            float progress = 0f;
            float totalWeight = 0f;

            for (int i = 0; i < inputCount; i++)
            {
                float weight = playable.GetInputWeight(i);
                if (weight <= 0f)
                    continue;

                ScriptPlayable<BoatMoveBehaviour> input = (ScriptPlayable<BoatMoveBehaviour>)playable.GetInput(i);
                BoatMoveBehaviour behaviour = input.GetBehaviour();
                float normalizedTime = (float)(input.GetTime() / input.GetDuration());

                progress += behaviour.Evaluate(normalizedTime) * weight;
                totalWeight += weight;
            }

            if (totalWeight > 0f)
                mover.SetProgress(progress / totalWeight);
        }
    }
}
