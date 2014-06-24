using MusicMinkAppLayer.Enums;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Models;
using System;
using System.Collections.Generic;

namespace MusicMink.ViewModels.MixEvaluators
{
    public enum NestedEvalType
    {
        Unknown,
        Any,
        All,
        None
    }

    class NestedMixEvaluator : IMixEvaluator
    {
        public MixType MixType { get; set; }

        public const string SplitToken = "|";

        public List<IMixEvaluator> Mixes { get; set; }

        public NestedEvalType EvalType { get; set; }

        public NestedMixEvaluator(List<IMixEvaluator> mixes, NestedEvalType evalType, MixType type)
        {
            Mixes = mixes;
            EvalType = evalType;
            MixType = type;
        }

        public bool Eval(SongViewModel song)
        {
            switch (EvalType)
            {
                case NestedEvalType.Any:
                    foreach (IMixEvaluator nestedMix in Mixes)
                    {
                        if (nestedMix.Eval(song))
                        {
                            return true;
                        }
                    }
                    return false;
                case NestedEvalType.All:
                    foreach (IMixEvaluator nestedMix in Mixes)
                    {
                        if (!nestedMix.Eval(song))
                        {
                            return false;
                        }
                    }
                    return true;
                case NestedEvalType.None:
                    foreach (IMixEvaluator nestedMix in Mixes)
                    {
                        if (nestedMix.Eval(song))
                        {
                            return false;
                        }
                    }
                    return true;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected NestedEvalType: {0}", EvalType);
                    return false;
            }
        }

        public bool IsMixNested(int targetId)
        {
            foreach (IMixEvaluator nestedMix in Mixes)
            {
                if (nestedMix.IsMixNested(targetId))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsPlaylistNested(int targetId)
        {
            foreach (IMixEvaluator nestedMix in Mixes)
            {
                if (nestedMix.IsPlaylistNested(targetId))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsPropertyAffected(string propertyName)
        {
            foreach (IMixEvaluator nestedMix in Mixes)
            {
                if (nestedMix.IsPropertyAffected(propertyName))
                {
                    return true;
                }
            }

            return false;
        }

        public int Save(int mixId, bool isRoot)
        {
            List<int> allIds = new List<int>();
            foreach (IMixEvaluator mix in Mixes)
            {
                allIds.Add(mix.Save(mixId, false));
            }

            return MixEntryModel.SaveMix(MixType, String.Join(SplitToken, allIds), mixId, isRoot);
        }
    }
}
