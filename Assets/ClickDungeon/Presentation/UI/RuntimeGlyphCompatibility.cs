using System.Text;
using TMPro;
using UnityEngine;

namespace ClickDungeon.Presentation.UI
{
    /// <summary>
    /// Keeps runtime-first UI markers readable on every shipped platform when the bundled
    /// TextMesh Pro font does not contain the decorative Unicode glyphs used by the board.
    /// The conversion is intentionally presentation-only and never changes simulation state.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public sealed class RuntimeGlyphCompatibility : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            var existing=UnityEngine.Object.FindAnyObjectByType<RuntimeGlyphCompatibility>();
            if(existing!=null)return;
            var host=new GameObject(nameof(RuntimeGlyphCompatibility));
            UnityEngine.Object.DontDestroyOnLoad(host);
            host.AddComponent<RuntimeGlyphCompatibility>();
        }

        private void LateUpdate()
        {
            var runtimeUi=UnityEngine.Object.FindAnyObjectByType<RuntimeGameUI>();
            if(runtimeUi==null)return;
            var labels=runtimeUi.GetComponentsInChildren<TMP_Text>(true);
            for(int i=0;i<labels.Length;i++)
            {
                var label=labels[i];
                if(label==null)continue;
                string normalized=Normalize(label.text);
                if(!string.Equals(normalized,label.text,System.StringComparison.Ordinal))label.text=normalized;
            }
        }

        internal static string Normalize(string value)
        {
            if(string.IsNullOrEmpty(value))return value;
            StringBuilder builder=null;
            for(int i=0;i<value.Length;i++)
            {
                string replacement=Replacement(value[i]);
                if(replacement==null)
                {
                    if(builder!=null)builder.Append(value[i]);
                    continue;
                }
                if(builder==null)
                {
                    builder=new StringBuilder(value.Length+8);
                    builder.Append(value,0,i);
                }
                builder.Append(replacement);
            }
            return builder==null?value:builder.ToString();
        }

        private static string Replacement(char value)
        {
            switch(value)
            {
                case '⚠':return "!";
                case '◆':return "@";
                case '✦':return "+";
                case '◇':return "O";
                case '✓':return "OK";
                case '·':return ".";
                case '☠':return "G";
                case '≈':return "=";
                case '♯':return "#";
                case 'ϟ':return "C";
                case '▲':return "^";
                case '✧':return "A";
                case '░':return ":";
                default:return null;
            }
        }
    }
}
