using MusicMinkAppLayer.Enums;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Models;
using System;
using System.Reflection;

namespace MusicMink.ViewModels.MixEvaluators
{
    public enum StringEvalType
    {
        Unknown,
        Equal,
        SubString,
        StartsWith,
        EndsWith
    }

    class StringMixEvaluator : IMixEvaluator
    {
        public MixType MixType { get; set; }

        public PropertyInfo TargetProperty { get; private set; }

        public string Target { get; private set; }

        public StringEvalType EvalType { get; private set; }

        public StringMixEvaluator(PropertyInfo targetProperty, string target, StringEvalType evalType, MixType type)
        {
            TargetProperty = targetProperty;
            Target = target;
            EvalType = evalType;
            MixType = type;
        }

        public bool Eval(SongViewModel song)
        {
            string targetValue = DebugHelper.CastAndAssert<string>(TargetProperty.GetValue(song));

            switch (EvalType)
            {
                case StringEvalType.Equal:
                    return targetValue == Target;
                case StringEvalType.SubString:
                    return targetValue.Contains(Target);
                case StringEvalType.StartsWith:
                    return targetValue.StartsWith(Target);
                case StringEvalType.EndsWith:
                    return targetValue.EndsWith(Target);
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected StringEvalType: {0}", EvalType);
                    return false;
            }
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
            return (TargetProperty.Name == propertyName);
        }

        public int Save(int mixId, bool isRoot)
        {
            return MixEntryModel.SaveMix(MixType, Target, mixId, isRoot);
        }

        public static MixType StringEvalTypeToMixTypeVariant(StringEvalType evalType)
        {
            switch (evalType)
            {
                case StringEvalType.EndsWith:
                    return MixType.STRING_ENDSWITH_VARIANT;
                case StringEvalType.StartsWith:
                    return MixType.STRING_STARTSWITH_VARIANT;
                case StringEvalType.SubString:
                    return MixType.STRING_CONTAINS_VARIANT;
                case StringEvalType.Equal:
                    return MixType.STRING_EQUAL_VARIANT;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected eval type");
                    return MixType.None;
            }
        }
    }
}
