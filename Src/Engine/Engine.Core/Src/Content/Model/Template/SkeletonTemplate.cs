using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Content.Model.Template
{
    public class SkeletonTemplate
    {
        private readonly Dictionary<string, SkeletonNodeTemplate> nodesByName;
        private readonly Dictionary<int, SkeletonNodeTemplate> nodesByIndex;

        private readonly AnimationTrackInfo[] animationTracks;

        public int Count => nodesByName.Count;
        public SkeletonNodeTemplate this[int index] => nodesByIndex[index];
        public SkeletonNodeTemplate this[string name] => nodesByName[name];

        public Dictionary<string, SkeletonNodeTemplate> NodesByName => nodesByName;
        public Dictionary<int, SkeletonNodeTemplate> NodesByIndex => nodesByIndex;

        public AnimationTrackInfo[] AnimationTracks => animationTracks;

        public SkeletonTemplate(SkeletonNodeTemplate[] nodes, AnimationTrackInfo[] animationTracks)
        {
            int i = 0;
            this.nodesByName = nodes.ToDictionary(k => k.Name, v => v);
            this.nodesByIndex = nodes.ToDictionary(k => i++, v => v);

            this.animationTracks = animationTracks;
        }

        public int IndexOfTrack(string name) => Array.FindIndex(animationTracks, item => item.Name == name);

        public float GetTrackDuration(int trackIndex)
        {
            if (trackIndex < 0)
                return 0;

            if (trackIndex >= animationTracks.Length)
                return 0;

            return animationTracks[trackIndex].Duration;
        }
    }
}
