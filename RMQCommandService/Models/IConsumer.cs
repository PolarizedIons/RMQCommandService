using System.Threading.Tasks;

namespace RMQCommandService.Models
{
    public interface IConsumer<in TCommand, TResult> where TCommand : ICommand
    {
        Task<TResult> HandleCommand(TCommand command);
    }
}
