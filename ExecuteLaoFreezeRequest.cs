namespace SPARTA_WAM.Models
{
    public class ExecuteLaoFreezeRequest1
    {
        public required string RegionCode { get; set; }
        public required List<string> SqlScripts { get; set; }
    }
}