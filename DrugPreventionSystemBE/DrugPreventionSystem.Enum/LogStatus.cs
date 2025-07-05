namespace DrugPreventionSystemBE.DrugPreventionSystem.Enum
{
    public enum LogStatus
    {
        // Payment Statuses
        PaymentPending = 1,
        PaymentSuccess = 2,
        PaymentFailed = 3,
        PaymentCanceled = 4,

        // Order Statuses
        OrderPending = 10,
        OrderPaid = 11,
        OrderCancelled = 12,
        OrderCompleted = 13,
        OrderFailed = 14,

        // Cart Statuses
        CartPending = 20,
        CartCompleted = 21,
        CartCanceled = 22
    }
}
