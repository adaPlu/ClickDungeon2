using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace ClickDungeon.Application.Replay
{
    public static class ReplayCodec
    {
        public static string Encode(ReplayEnvelope replay)
        {
            if(replay==null) throw new ArgumentNullException(nameof(replay));
            byte[] raw=Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(replay,Formatting.None));
            byte[] packed;
            using(var output=new MemoryStream())
            {
                using(var gzip=new GZipStream(output,CompressionLevel.Optimal,true)) gzip.Write(raw,0,raw.Length);
                packed=output.ToArray();
            }
            return Convert.ToBase64String(packed).TrimEnd('=').Replace('+','-').Replace('/','_');
        }

        public static ReplayEnvelope Decode(string encoded)
        {
            if(string.IsNullOrWhiteSpace(encoded)) throw new ArgumentException("Replay string is empty.",nameof(encoded));
            string base64=encoded.Replace('-','+').Replace('_','/'); while(base64.Length%4!=0) base64+="=";
            byte[] packed=Convert.FromBase64String(base64); using(var input=new MemoryStream(packed)) using(var gzip=new GZipStream(input,CompressionMode.Decompress)) using(var output=new MemoryStream())
            {
                gzip.CopyTo(output); string json=Encoding.UTF8.GetString(output.ToArray()); return JsonConvert.DeserializeObject<ReplayEnvelope>(json)??throw new InvalidDataException("Replay payload invalid.");
            }
        }
    }
}
