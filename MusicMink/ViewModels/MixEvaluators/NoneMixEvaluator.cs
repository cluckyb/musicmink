using MusicMinkAppLayer.Enums;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Models;
using System;

namespace MusicMink.ViewModels.MixEvaluators
{
    class NoneMixEvaluator : IMixEvaluator
    {
        public MixType MixType { get; set; }

        public NoneMixEvaluator()
        {
            MixType = MixType.None;
        }

        public bool Eval(SongViewModel song)
        {
            return false;
        }

        public bool IsMixNested(int targetId)
        {
            return false;
        }

        public bool IsPlaylistNested(int targetId)
        {
            return false;
        }

        public bool IsPropertyAffected(string propertyName)
        {
            return false;
        }

        public int Save(int mixId, bool isRoot)
        {
            return MixEntryModel.SaveMix(MixType, string.Empty, mixId, isRoot);
        }
    }
}
