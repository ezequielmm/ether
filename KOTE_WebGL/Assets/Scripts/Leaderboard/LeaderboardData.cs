namespace DefaultNamespace.Leaderboard.Old
{
    using System.Collections.Generic;

    public class DataItem
    {
        public int score { get; set; }
        public string address { get; set; }
    }

    public class Pagination
    {
        public int currentPage { get; set; }
        public int pageSize { get; set; }
        public int totalItems { get; set; }
        public int totalPages { get; set; }
    }

    public class Root
    {
        public DataItem[] data { get; set; }
        public Pagination pagination { get; set; }
    }
}