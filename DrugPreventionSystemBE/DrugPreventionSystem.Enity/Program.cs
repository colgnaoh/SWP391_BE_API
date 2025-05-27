using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Enity
{
    public class Program : BaseEnity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public Programtype Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}
