namespace BE.Services
{
    public interface IMail
    {
        long Id { get; set; }
        ISender Sender { get; set; }
    }


}
