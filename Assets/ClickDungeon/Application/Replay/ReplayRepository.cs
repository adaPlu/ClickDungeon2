using System;
using System.IO;
using System.Text;
using UnityEngine;
using ClickDungeon.Application.Platform;

namespace ClickDungeon.Application.Replay
{
    public sealed class ReplayRepository
    {
        private readonly string _directory;
        public ReplayRepository(string directory=null){_directory=directory??Path.Combine(UnityEngine.Application.persistentDataPath,"ClickDungeon2","replays");}

        public void SaveLast(ReplayEnvelope replay)
        {
            if(replay==null)throw new ArgumentNullException(nameof(replay));
            Directory.CreateDirectory(_directory);
            string encoded=ReplayCodec.Encode(replay);
            string primary=Path.Combine(_directory,"last.replay");string temp=primary+".tmp";string backup=primary+".bak";
            File.WriteAllText(temp,encoded,Encoding.UTF8);
            ReplayCodec.Decode(File.ReadAllText(temp,Encoding.UTF8));
            if(File.Exists(primary))File.Copy(primary,backup,true);
            File.Copy(temp,primary,true);File.Delete(temp);PersistentDataSync.RequestSync();
        }

        public ReplayEnvelope LoadLast()
        {
            string primary=Path.Combine(_directory,"last.replay");string backup=primary+".bak";Exception last=null;
            foreach(string path in new[]{primary,backup})
            {
                if(!File.Exists(path))continue;
                try{return ReplayCodec.Decode(File.ReadAllText(path,Encoding.UTF8));}catch(Exception ex){last=ex;}
            }
            if(last!=null)throw new InvalidDataException("No valid replay copy available.",last);
            return null;
        }

        public void DeleteLast()
        {
            foreach(string suffix in new[]{"",".bak",".tmp"}){string path=Path.Combine(_directory,"last.replay"+suffix);if(File.Exists(path))File.Delete(path);}PersistentDataSync.RequestSync();
        }
    }
}
