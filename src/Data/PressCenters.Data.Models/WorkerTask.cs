namespace PressCenters.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using PressCenters.Data.Common.Models;

    public class WorkerTask : BaseModel<int>
    {
        public WorkerTask()
        {
        }

        public WorkerTask(WorkerTask existingTask, DateTime runAfter)
            : this()
        {
            this.TypeName = existingTask.TypeName;
            this.Parameters = existingTask.Parameters;
            this.Priority = existingTask.Priority;
            this.RunAfter = runAfter;
        }

        public WorkerTask(WorkerTask existingTask, string parameters, DateTime runAfter)
            : this()
        {
            this.TypeName = existingTask.TypeName;
            this.Parameters = parameters;
            this.Priority = existingTask.Priority;
            this.RunAfter = runAfter;
        }

        [Required]
        [MaxLength(100)]
        public string TypeName { get; set; }

        public string Parameters { get; set; }

        public DateTime? RunAfter { get; set; }

        public int Priority { get; set; }

        public bool Processing { get; set; }

        public bool Processed { get; set; }

        public string ProcessingComment { get; set; }

        public string Result { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
