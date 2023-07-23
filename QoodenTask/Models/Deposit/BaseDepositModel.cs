namespace QoodenTask.Models.Deposit;

public class BaseDepositModel
{
    private decimal _amount;
    public decimal Amount
    {
        get { return _amount;}
        set
        {
            if (value < 0.1m || value > 100)
                throw new ArgumentException("Not valid amount");
            else
                _amount = value;
        }
    }
}