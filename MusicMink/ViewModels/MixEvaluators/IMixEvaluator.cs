using MusicMinkAppLayer.Enums;
using System.Reflection;

namespace MusicMink.ViewModels.MixEvaluators
{
    public interface IMixEvaluator
    {
        MixType MixType { get; set; }

        bool Eval(SongViewModel song);

        bool IsMixNested(int targetId);

        bool IsPlaylistNested(int targetId);

        bool IsPropertyAffected(string propertyName);

        int Save(int rootId, bool isRoot);
    }
}
