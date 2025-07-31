using System;

namespace RivenFramework
{
    public class TodoAttribute : Attribute
    {
        protected string forWho;
        protected string description;

        public TodoAttribute(string description, string forWho = "All")
        {
            this.forWho = forWho.ToLower();
            this.description = description;
        }

        public virtual string GetDescription() => description;
    }

    public class Todo_AddCommentsAttribute : TodoAttribute
    {
        public Todo_AddCommentsAttribute(string forWho = "All") 
            : base("Add comments and/or summaries", forWho) { }
    }
    public class Todo_OptimizeAttribute : TodoAttribute
    {
        public Todo_OptimizeAttribute(string forWho = "All")
            : base("Optimize code", forWho) { }
    }
    public class Todo_StressTestAttribute : TodoAttribute
    {
        public Todo_StressTestAttribute(string forWho = "All")
            : base("Stress test code", forWho) { }
    }
    public class Todo_ImplementAttribute : TodoAttribute
    {
        public Todo_ImplementAttribute(string forWho = "All")
            : base("Finish implementation", forWho) { }
    }
    public class Todo_PoorlyCodedAttribute : TodoAttribute
    {
        public Todo_PoorlyCodedAttribute(string forWho = "All")
            : base("Fix poor implementation", forWho) { }
    }
    public class Todo_ToRemoveAttribute : TodoAttribute
    {
        public Todo_ToRemoveAttribute(string forWho = "All")
            : base("Remove implementation", forWho) { }
    }
}
