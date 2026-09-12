using Cortex.Mediator.Commands;

namespace MaleFashion.Application.Features.Carts.Commands
{
    public class CartClearStaleCommand : ICommand<int>
    {
        public int OlderThanDays { get; set; } = 30;

        public CartClearStaleCommand(int olderThanDays)
        {
            OlderThanDays = olderThanDays;
        }

        public CartClearStaleCommand() { }
    }
}

