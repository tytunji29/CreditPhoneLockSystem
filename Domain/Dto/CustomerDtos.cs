namespace Domain.Dto;
public class CreateCustomerDto
{
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string IMEI { get; set; }

    public decimal AmountPaid { get; set; }
    public decimal AmountToBalance { get; set; }
    public int NumberOfRepaymentMonths { get; set; }
}

public class CustomerResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string IMEI { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<RepaymentScheduleResponseDto> Repay { get; set; }
}
