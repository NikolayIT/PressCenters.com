namespace PressCenters.Worker.Common
{
    using System;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using PressCenters.Data.Models;

    public abstract class BaseTask<TInput, TOutput> : ITask
        where TInput : BaseTaskInput
        where TOutput : BaseTaskOutput, new()
    {
        protected BaseTask(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        protected IServiceProvider ServiceProvider { get; }

        public async Task<string> DoWork(string parameters)
        {
            var taskParameters = JsonConvert.DeserializeObject<TInput>(parameters);
            TOutput taskResult;
            try
            {
                taskResult = await this.DoWork(taskParameters);
            }
            catch (Exception ex)
            {
                taskResult = new TOutput { Ok = false, Error = ex.ToString() };
            }

            var taskResultAsString = JsonConvert.SerializeObject(taskResult);
            return taskResultAsString;
        }

        public WorkerTask Recreate(WorkerTask currentTask)
        {
            var taskParameters = JsonConvert.DeserializeObject<TInput>(currentTask.Parameters);
            return taskParameters.Recreate ? this.Recreate(currentTask, taskParameters) : null;
        }

        protected virtual WorkerTask Recreate(WorkerTask currentTask, TInput parameters) => null; // Returning null means no recreation

        protected abstract Task<TOutput> DoWork(TInput input);
    }
}
