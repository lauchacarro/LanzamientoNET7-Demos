namespace WebApiDemo7.Models
{
    public interface IDateTime
    {
        DateTime GetNow();
    }

    internal class DateTimeManager : IDateTime
    {
        public DateTime GetNow()
        {
            return DateTime.Now;
        }
    }
}
