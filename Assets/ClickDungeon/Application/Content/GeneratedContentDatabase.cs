using System;
using System.Collections.Generic;
using UnityEngine;
using ClickDungeon.Simulation.Content;

namespace ClickDungeon.Application.Content
{
    [CreateAssetMenu(menuName="ClickDungeon/Generated Content Database",fileName="ClickDungeonGeneratedContent")]
    public sealed class GeneratedContentDatabase : ScriptableObject
    {
        [Serializable]
        public sealed class Document
        {
            public string FileName = string.Empty;
            [TextArea(4,20)] public string Json = string.Empty;
        }

        [SerializeField] private int contentRevision = 1;
        [SerializeField] private List<Document> documents = new List<Document>();
        public int ContentRevision => contentRevision;
        public IReadOnlyList<Document> Documents => documents;

        public void ReplaceDocuments(int revision,List<Document> values)
        {
            contentRevision=revision;documents=values??new List<Document>();
        }

        public GameContent CreateCatalog()
        {
            var map=new Dictionary<string,string>(StringComparer.Ordinal);
            foreach(var doc in documents)if(doc!=null&&!string.IsNullOrWhiteSpace(doc.FileName))map[doc.FileName]=doc.Json??string.Empty;
            return new JsonContentCatalogLoader().LoadFromDocuments(map);
        }
    }
}
