namespace ExploringKafkaEventProcessing;

public class BankAccount
{
    public decimal Balance { get; private set; }

    public void Apply(MoneyDeposited moneyDeposited) =>
        Balance += moneyDeposited.Amount;
}