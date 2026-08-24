using Newtonsoft.Json;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Application.Persistence;

namespace ClickDungeon.Application.Replay
{
    public static class StateHasher
    {
        public static string Hash(RunState state)
        {
            var json=JsonConvert.SerializeObject(state,Formatting.None,new JsonSerializerSettings{ReferenceLoopHandling=ReferenceLoopHandling.Error});
            return ChecksumUtility.Sha256(json);
        }
    }
}
