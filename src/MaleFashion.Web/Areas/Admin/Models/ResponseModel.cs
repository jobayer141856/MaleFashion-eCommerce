namespace MaleFashion.Web.Areas.Admin.Models
{
    public enum ResponseTypes
    {
        Success,
        Danger,
        Warning,
        Info
    }

    public class ResponseModel
    {
        public string? Message { get; set; }
        public ResponseTypes Type { get; set; }
    }
}
