namespace QDMS.Classes
{
    public static class DocumentUtility
    {
        public static string GetDocumentTypePrefix(int type)
        {
            return new Dictionary<int, string>()
            {
                [0] = "EK",
                [1] = "PR",
                [2] = "FR",
                [3] = "SZ"
            }.TryGetValue(type, out var val) ? val : "UNK";
        }
    }
}
