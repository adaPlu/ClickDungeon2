using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using ClickDungeon.Application.State;
using ClickDungeon.Application.Platform;

namespace ClickDungeon.Application.Persistence
{
    public sealed class AccountRepository
    {
        private readonly string _path;
        public AccountRepository(string path=null) { _path=path??Path.Combine(UnityEngine.Application.persistentDataPath,"ClickDungeon2","account.json"); }
        public AccountState Load()
        {
            Exception last=null;foreach(string path in new[]{_path,_path+".bak"}){if(!File.Exists(path))continue;try{return JsonConvert.DeserializeObject<AccountState>(File.ReadAllText(path,Encoding.UTF8))??new AccountState();}catch(Exception ex){last=ex;}}
            if(last!=null)Debug.LogError($"Account settings and backup were unreadable; defaults loaded. {last}");return new AccountState();
        }
        public void Save(AccountState state)
        {
            if(state==null) throw new ArgumentNullException(nameof(state));Directory.CreateDirectory(Path.GetDirectoryName(_path));string tmp=_path+".tmp";string backup=_path+".bak";string json=JsonConvert.SerializeObject(state,Formatting.Indented);File.WriteAllText(tmp,json,Encoding.UTF8);JsonConvert.DeserializeObject<AccountState>(File.ReadAllText(tmp,Encoding.UTF8));if(File.Exists(_path))File.Copy(_path,backup,true);File.Copy(tmp,_path,true);File.Delete(tmp);PersistentDataSync.RequestSync();
        }
    }
}
