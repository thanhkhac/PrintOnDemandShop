namespace CleanArchitectureBase.Application.Vouchers.Command;

public class CreateVoucherCommand
{
    public string? Code { get; set; }
    public string? Description { get; set; }  
    public int DiscountAmount { get; set; }
    public int DiscountPercent { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
         
}
