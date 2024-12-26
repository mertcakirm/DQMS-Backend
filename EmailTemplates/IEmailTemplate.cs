namespace QDMS.EmailTemplates
{
    public interface IEmailTemplate
    {
        string EmailTitle { get; }
        bool IsHtml { get; }
        string GetBody();
    }
}
