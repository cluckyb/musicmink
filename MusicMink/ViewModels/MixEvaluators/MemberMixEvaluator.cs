using MusicMinkAppLayer.Enums;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Models;
using System;

namespace MusicMink.ViewModels.MixEvaluators
{
    public enum MemberEvalType
    {
        Unknown,
        Playlist,
        Mix,
    }

    class MemberMixEvaluator : IMixEvaluator
    {
        public MixType MixType { get; set; }

        public int Target { get; private set; }

        public MemberEvalType EvalType { get; private set; }

        public MemberMixEvaluator(int target, MemberEvalType evalType, MixType type)
        {
            Target = target;
            EvalType = evalType;
            MixType = type;
        }

        public bool Eval(SongViewModel song)
        {
            switch (EvalType)
            {
                case MemberEvalType.Playlist:
                    PlaylistViewModel playlist = LibraryViewModel.Current.LookupPlaylistById(Target);
                    if (playlist == null)
                    {
                        return false;
                    }
                    return playlist.ContainsSong(song);
                case MemberEvalType.Mix:
                    MixViewModel mix = LibraryViewModel.Current.LookupMixById(Target);
                    if (mix == null)
                    {
                        return false;
                    }
                    return mix.ContainsSong(song);
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected MemberEvalType: {0}", EvalType);
                    return false;
            }
        }

        public bool IsMixNested(int targetId)
        {
            return (EvalType == MemberEvalType.Mix && targetId == Target);
        }

        public bool IsPlaylistNested(int targetId)
        {
            return (EvalType == MemberEvalType.Playlist && targetId == Target);
        }

        public bool IsPropertyAffected(string propertyName)
        {
            // If the nested mix is affected, is covered by IsMixNested
            return false;
        }

        public int Save(int mixId, bool isRoot)
        {
            return MixEntryModel.SaveMix(MixType, Target.ToString(), mixId, isRoot);
        }
    }
}
